
namespace MedicLaunchApi.Storage
{
    public interface IAzureBlobClient
    {
        // Create a generic blob client interface that can be used to upload and download blobs from Azure Blob Storage. Call each object an item

        Task<TItem> GetItemAsync<TItem>(string fullPath, CancellationToken cancellationToken, bool ignoreNotFound = false);

        Task<TItem> CreateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null);

        Task<TItem> UpdateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null);

        Task DeleteItemAsync(string fullPath, CancellationToken cancellationToken);

        Task<IEnumerable<TItem>> GetAllItemsAsync<TItem>(string folderPath, CancellationToken cancellationToken);

        Task<TItem> CreateOrUpdateItem<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags = null);
        Task<string> UploadImageAsync(IFormFile file);
    }
}
