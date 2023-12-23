using MedicLaunchApi.Models;
using MedicLaunchApi.Storage;

namespace MedicLaunchApi.Repository
{
    public class QuestionRepository
    {
        private readonly AzureBlobClient azureBlobClient;

        public QuestionRepository(AzureBlobClient azureBlobClient)
        {
            this.azureBlobClient = azureBlobClient;
        }
    }
}
