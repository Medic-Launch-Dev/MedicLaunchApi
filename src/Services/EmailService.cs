using SendGrid;
using SendGrid.Helpers.Mail;

namespace MedicLaunchApi.Services
{
	public class EmailService
	{
		private readonly string sendGridApiKey;
		private readonly string fromEmail;
		private readonly string fromName;
		private readonly ILogger<EmailService> logger;

		public EmailService(ILogger<EmailService> logger)
		{
			this.sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ??
				throw new ArgumentNullException("SENDGRID_API_KEY environment variable is not set");
			this.fromEmail = "info@mediclaunch.com";
			this.fromName = "Medic Launch";
			this.logger = logger;
		}

		public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
		{
			var client = new SendGridClient(sendGridApiKey);
			var from = new EmailAddress(fromEmail, fromName);
			var to = new EmailAddress(email);
			var subject = "Confirm your MedicLaunch account";
			var plainTextContent = $"Please confirm your account by clicking this link: {confirmationLink}";
			var htmlContent = $@"
                <h2>Welcome to MedicLaunch!</h2>
                <p>Please confirm your account by clicking the button below:</p>
                <p>
                    <a href='{confirmationLink}' style='padding: 10px 20px; background: #007bff; color: white; text-decoration: none; border-radius: 5px;'>
                        Confirm Email
                    </a>
                </p>";

			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

			try
			{
				var response = await client.SendEmailAsync(msg);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogError($"Failed to send confirmation email to {email}");
					throw new Exception("Failed to send confirmation email");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Error sending confirmation email to {email}");
				throw;
			}
		}
	}
}