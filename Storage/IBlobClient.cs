namespace MedicLaunchApi.Storage
{
    public interface IBlobClient
    {
        // Create a generic blob client interface that can be used to upload and download blobs from Azure Blob Storage. Call each object an item

        Task<TItem> GetItemAsync<TItem>(string fullPath, CancellationToken cancellationToken, bool ignoreNotFound = false);

        Task<TItem> CreateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags);

        Task<TItem> UpdateItemAsync<TItem>(string fullPath, TItem item, CancellationToken cancellationToken, Dictionary<string, string> tags);

        Task DeleteItemAsync(string fullPath, CancellationToken cancellationToken);

        // write task method to get all items
        Task<IEnumerable<TItem>> GetAllItemsAsync<TItem>(string folderPath, CancellationToken cancellationToken);
    }
}
