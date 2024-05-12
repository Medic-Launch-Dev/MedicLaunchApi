
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace MedicLaunchApi.Storage
{
    public class AzureBlobClient: IAzureBlobClient
    {
        private readonly BlobContainerClient blobContainerClient;
        private readonly BlobContainerClient imagesContainerClient;
        private readonly ILogger<AzureBlobClient> logger;

        public AzureBlobClient(ILogger<AzureBlobClient> logger)
        {
            string? connectionStringFromEnvironment = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if(string.IsNullOrEmpty(connectionStringFromEnvironment))
            {
                throw new Exception("AZURE_STORAGE_CONNECTION_STRING environment variable is not set");
            }

            this.blobContainerClient = new BlobContainerClient(connectionStringFromEnvironment, "database");
            this.imagesContainerClient = new BlobContainerClient(connectionStringFromEnvironment, "images");
            this.imagesContainerClient.CreateIfNotExists(PublicAccessType.BlobContainer);
            this.logger = logger;
        }

        public async Task<TItem> CreateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null)
        {
            this.logger.LogInformation($"Creating blob at {fullPath}");
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            var exists = await blobClient.ExistsAsync(cancellationToken);
            if (exists.HasValue && exists.Value)
            {
                throw new Exception($"Blob with the same name already exists at {fullPath}");
            }

            return await UploadItemAsync(blobClient, item, cancellationToken, tags);
        }

        public async Task<TItem> CreateOrUpdateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null)
        {
            this.logger.LogInformation($"Creating or updating blob at {fullPath}");
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            
            var exists = await blobClient.ExistsAsync(cancellationToken);

            var verb = exists.HasValue && exists.Value ? "Updating" : "Creating";
            this.logger.LogInformation($"{verb} blob at {fullPath}");

            return await UploadItemAsync(blobClient, item, cancellationToken, tags);
        }

        public async Task DeleteItemAsync(string fullPath, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Deleting blob at {fullPath}");
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }

        // TODO: add filter to the GetAllItemsAsync method to only return items that match the filter
        public async Task<IEnumerable<TItem>> GetAllItemsAsync<TItem>(string folderPath, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Getting all blobs at {folderPath}");

            var pages = blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken, traits: BlobTraits.Tags, prefix: folderPath).AsPages();

            var blobs = new ConcurrentBag<TItem>();
            await foreach (var page in pages)
            {
                foreach (var blob in page.Values)
                {
                    var item = await GetItemAsync<TItem>(blob.Name, cancellationToken, false);
                    blobs.Add(item);
                }
            }

            return blobs;
        }

        public async Task<TItem> GetItemAsync<TItem>(string fullPath, CancellationToken cancellationToken, bool ignoreNotFound)
        {
            this.logger.LogInformation($"Getting blob at {fullPath}");
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            var exists = await blobClient.ExistsAsync(cancellationToken);
            if (exists.HasValue && exists.Value)
            {
                var blobReadOptions = new BlobOpenReadOptions(false);
                using var stream = await blobClient.OpenReadAsync(blobReadOptions, cancellationToken);
                var result = await JsonSerializer.DeserializeAsync<TItem>(stream, cancellationToken: cancellationToken);
                return result;
            }

            if (ignoreNotFound)
            {
                return default;
            }

            string message = $"Blob not found at {fullPath}";
            this.logger.LogError(message);
            throw new KeyNotFoundException(message);
        }

        public async Task<TItem> UpdateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null)
        {
            this.logger.LogInformation($"Updating blob at {fullPath}");
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            var exists = await blobClient.ExistsAsync(cancellationToken);

            if (exists.HasValue && exists.Value)
            {
                return await UploadItemAsync(blobClient, item, cancellationToken, tags);
            }

            string message = $"Blob not found at {fullPath}";
            this.logger.LogError(message);
            throw new Exception(message);
        }

        public async Task<TItem> CreateOrUpdateItem<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null)
        {
            var blobClient = this.blobContainerClient.GetBlobClient(fullPath);
            var exists = await blobClient.ExistsAsync(cancellationToken);

            var logMessage = exists.HasValue && exists.Value ? $"Updating blob at {fullPath}" : $"Creating blob at {fullPath}";
            this.logger.LogInformation(logMessage);
            return await UploadItemAsync(blobClient, item, cancellationToken, tags);
        }

        public async Task<string> UploadImageAsyc(IFormFile file)
        {
            this.logger.LogInformation($"Uploading file {file.FileName}");
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = imagesContainerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            var blobUri = blobClient.Uri.ToString();

            return blobUri;
        }

        private static async Task<TItem> UploadItemAsync<TItem>(BlobClient blobClient, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags)
        {
            var json = JsonSerializer.Serialize(item);
            var bytes = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(bytes);

            var uploadOptions = new BlobUploadOptions();
            if(tags != null)
            {
                uploadOptions.Tags = tags;
            }

            uploadOptions.HttpHeaders = new BlobHttpHeaders()
            {
                ContentType = "application/json"
            };

            await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
            return item;
        }
    }
}
