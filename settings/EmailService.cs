using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;



namespace WebApplication2.settings {

	public class EmailService {

		private readonly EmailSettings _settings;
		public EmailService(IOptions<EmailSettings> opts)
		{
			_settings = opts.Value;
		}

		public async Task SendVerificationCodeAsync(string toEmail, string code)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
			message.To.Add(MailboxAddress.Parse(toEmail));
			message.Subject = "Verification Code";

			message.Body = new TextPart("plain")
			{
				Text = $"Your verification code is: {code}\nIt expires in 5 minutes."
			};

			using var smtp = new SmtpClient();
			// StartTLS on port 587
			await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
			await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
			await smtp.SendAsync(message);
			await smtp.DisconnectAsync(true);
		}
	}
}