using MedicLaunchApi.Models;
using MedicLaunchApi.Storage;

namespace MedicLaunchApi.Repository
{
    public class PaymentRepository
    {
        private readonly ILogger<PaymentRepository> logger;
        private readonly AzureBlobClient azureBlobClient;

        public PaymentRepository(ILogger<PaymentRepository> logger, AzureBlobClient azureBlobClient)
        {
            this.logger = logger;
            this.azureBlobClient = azureBlobClient;
        }

        public async Task CreatePayment(Payment payment, string userId, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Creating payment for user {userId}");
            var paymentJsonPath = GetPaymentsJsonPath(userId);
            var payments = await azureBlobClient.GetItemAsync<List<Payment>>(paymentJsonPath, cancellationToken, true);

            if (payments == null)
            {
                payments = new List<Payment> { payment };
                await azureBlobClient.CreateItemAsync(paymentJsonPath, payments, cancellationToken);
                return;
            }

            payments.Add(payment);
            await azureBlobClient.UpdateItemAsync(paymentJsonPath, payments, cancellationToken);

            this.logger.LogInformation($"Payment for user {userId} created");
        }

        public async Task<IEnumerable<Payment>> GetPayments(string userId, CancellationToken cancellationToken)
        {
            var paymentJsonPath = GetPaymentsJsonPath(userId);
            return await azureBlobClient.GetItemAsync<List<Payment>>(paymentJsonPath, cancellationToken, true);
        }

        public async Task UpdatePaymentStatus(string userId, string paymentIntentId, string status, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Updating payment status for user {userId}");
            var paymentJsonPath = GetPaymentsJsonPath(userId);
            var payments = await azureBlobClient.GetItemAsync<List<Payment>>(paymentJsonPath, cancellationToken, true);

            if (payments == null)
            {
                throw new Exception($"No payments found for user {userId}");
            }

            var payment = payments.FirstOrDefault(p => p.PaymentIntentId == paymentIntentId);
            if (payment == null)
            {
                throw new Exception($"No payment intent found with paymentIntentId {paymentIntentId}");
            }

            payment.PaymentIntentStatus = status;
            await azureBlobClient.UpdateItemAsync(paymentJsonPath, payments, cancellationToken);

            this.logger.LogInformation($"Payment status for user {userId} updated");
        }

        public string GetPaymentsJsonPath(string userId)
        {
            return $"{userId}/payments.json";
        }
    }
}
