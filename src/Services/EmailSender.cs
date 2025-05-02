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
		var firstName = user?.GetType().GetProperty("FirstName")?.GetValue(user)?.ToString() ?? "there";

		var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <title>Welcome to Medic Launch – Confirm Your Email</title>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f9fafb;
      margin: 0;
      padding: 0;
      color: #333;
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      padding: 30px;
      background-color: #ffffff;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }}
    .header {{
      text-align: center;
      padding-bottom: 20px;
    }}
    .header img {{
      max-height: 50px;
    }}
    .content {{
      font-size: 16px;
      line-height: 1.6;
    }}
    .button {{
      display: inline-block;
      margin-top: 30px;
      padding: 12px 24px;
      font-size: 16px;
      background-color: #2394c4;
      color: #ffffff;
      text-decoration: none;
      border-radius: 6px;
    }}
    .footer {{
      text-align: center;
      font-size: 13px;
      color: #888;
      margin-top: 40px;
    }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <img src=""https://framerusercontent.com/images/wBeRzuky13kx8AHKqs4j1nVYD4.png?scale-down-to=512"" alt=""Medic Launch Logo"" />
    </div>
    <div class=""content"">
      <p>Hi <strong>{firstName}</strong>,</p>
      <p>Welcome to Medic Launch! 🚀</p>
      <p>We're thrilled to have you on board as you prepare for your journey to success in the UKMLA.</p>
      <p><strong>Please confirm your email address</strong> to activate your account and access everything Medic Launch has to offer.</p>
      <p style=""text-align: center;"">
        <a href=""{confirmationLink}"" class=""button"" style=""color: #ffffff !important; text-decoration: none;"">Confirm My Email</a>
      </p>
      <p>Supporting your journey,<br><strong>The Medic Launch Team</strong></p>
    </div>
    <div class=""footer"">
      © 2025 Medic Launch. All rights reserved.
    </div>
  </div>
</body>
</html>";

		await ExecuteEmailAsync(
			email,
			"Confirm your email",
			htmlBody
		);
	}

	public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
	{
		// Try to get the user's first name if possible, otherwise use a fallback
		var firstName = user?.GetType().GetProperty("FirstName")?.GetValue(user)?.ToString() ?? "there";

		var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <title>Reset Your Password</title>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f9fafb;
      margin: 0;
      padding: 0;
      color: #333;
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      padding: 30px;
      background-color: #ffffff;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.05);
    }}
    .header {{
      text-align: center;
      padding-bottom: 20px;
    }}
    .header img {{
      max-height: 50px;
    }}
    .content {{
      font-size: 16px;
      line-height: 1.6;
    }}
    .button {{
      display: inline-block;
      margin-top: 30px;
      padding: 12px 24px;
      font-size: 16px;
      background-color: #2394c4;
      color: #ffffff;
      text-decoration: none;
      border-radius: 6px;
    }}
    .footer {{
      text-align: center;
      font-size: 13px;
      color: #888;
      margin-top: 40px;
    }}
  </style>
</head>
<body>
  <div class=""container"">
    <div class=""header"">
      <img src=""https://framerusercontent.com/images/wBeRzuky13kx8AHKqs4j1nVYD4.png?scale-down-to=512"" alt=""Medic Launch Logo"" />
    </div>
    <div class=""content"">
      <p>Hi <strong>{firstName}</strong>,</p>
      <p>We received a request to reset your Medic Launch password. No worries — it happens to the best of us!</p>
      <p>Click the button below to create a new password:</p>
      <p style=""text-align: center;"">
        <a href=""{resetLink}"" class=""button"" style=""color: #ffffff !important; text-decoration: none;"">Reset My Password</a>
      </p>
      <p>If you didn’t request this, you can safely ignore this email. Your current password will remain unchanged.</p>
      <p>Need help or have any questions? Just reply to this email — we're here for you.</p>
      <p>Wishing you success on your medical journey,<br><strong>The Medic Launch Team</strong></p>
    </div>
    <div class=""footer"">
      © 2025 Medic Launch. All rights reserved.
    </div>
  </div>
</body>
</html>";

		await ExecuteEmailAsync(
			email,
			"Reset your password",
			htmlBody
		);
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