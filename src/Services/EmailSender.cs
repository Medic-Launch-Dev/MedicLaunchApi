using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MedicLaunchApi.Services;

public class EmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
	private readonly ILogger _logger;
	private readonly string _sendGridKey;

	public EmailSender(ILogger<EmailSender<TUser>> logger)
	{
		_logger = logger;
		_sendGridKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ??
			throw new InvalidOperationException("SENDGRID_API_KEY environment variable is not set");
	}

	public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
	{
		_logger.LogInformation($"Sending confirmation link to {email}");
		await ExecuteEmailAsync(
			email,
			"Confirm your email",
			$"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
	}

	public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
	{
		await ExecuteEmailAsync(
			email,
			"Reset your password",
			$"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
	}

	public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
	{
		await ExecuteEmailAsync(
			email,
			"Reset your password",
			$"Your password reset code is: {resetCode}");
	}

	private async Task ExecuteEmailAsync(string toEmail, string subject, string message)
	{
		var client = new SendGridClient(_sendGridKey);
		var msg = new SendGridMessage()
		{
			From = new EmailAddress("info@mediclaunch.co.uk", "Medic Launch"),
			Subject = subject,
			PlainTextContent = message,
			HtmlContent = message
		};
		msg.AddTo(new EmailAddress(toEmail));

		msg.SetClickTracking(false, false);
		var response = await client.SendEmailAsync(msg);

		if (response.IsSuccessStatusCode)
		{
			_logger.LogInformation($"Email to {toEmail} queued successfully!");
		}
		else
		{
			var errorBody = await response.Body.ReadAsStringAsync();
			_logger.LogError($"Failure Email to {toEmail}. Status: {response.StatusCode}. Error: {errorBody}");
		}
	}
}