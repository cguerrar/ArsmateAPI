using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Arsmate.Core.Interfaces;

namespace Arsmate.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            _logger.LogInformation($"Sending email to {to} with subject: {subject}");
            // TODO: Implementar con SendGrid o SMTP
            await Task.Delay(100); // Simular envío
            return true;
        }

        public async Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true)
        {
            foreach (var recipient in to)
            {
                await SendEmailAsync(recipient, subject, body, isHtml);
            }
            return true;
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken)
        {
            var subject = "Confirm your Arsmate account";
            var body = $@"
                <h2>Welcome to Arsmate!</h2>
                <p>Please confirm your email by clicking the link below:</p>
                <a href='https://arsmate.com/confirm-email?token={confirmationToken}&email={email}'>Confirm Email</a>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var subject = "Reset your Arsmate password";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Click the link below to reset your password:</p>
                <a href='https://arsmate.com/reset-password?token={resetToken}&email={email}'>Reset Password</a>
                <p>If you didn't request this, please ignore this email.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string username)
        {
            var subject = "Welcome to Arsmate International!";
            var body = $@"
                <h2>Welcome {username}!</h2>
                <p>Thank you for joining Arsmate International.</p>
                <p>Start exploring amazing content from creators worldwide!</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendSubscriptionConfirmationAsync(string email, string creatorName, decimal amount)
        {
            var subject = $"Subscription to {creatorName} confirmed";
            var body = $@"
                <h2>Subscription Confirmed</h2>
                <p>You are now subscribed to {creatorName} for ${amount}/month.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendSubscriptionCancelledAsync(string email, string creatorName)
        {
            var subject = $"Subscription to {creatorName} cancelled";
            var body = $@"
                <p>Your subscription to {creatorName} has been cancelled.</p>
                <p>You will have access until the end of your billing period.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendSubscriptionExpiringAsync(string email, string creatorName, int daysRemaining)
        {
            var subject = $"Your subscription to {creatorName} is expiring soon";
            var body = $@"
                <p>Your subscription to {creatorName} will expire in {daysRemaining} days.</p>
                <p>Renew now to keep enjoying exclusive content!</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendNewSubscriberNotificationAsync(string creatorEmail, string subscriberName)
        {
            var subject = "You have a new subscriber!";
            var body = $@"
                <h2>Congratulations!</h2>
                <p>{subscriberName} just subscribed to your content.</p>
            ";
            return await SendEmailAsync(creatorEmail, subject, body);
        }

        public async Task<bool> SendWithdrawalProcessedAsync(string email, decimal amount, string method)
        {
            var subject = "Your withdrawal has been processed";
            var body = $@"
                <p>Your withdrawal of ${amount} via {method} has been processed.</p>
                <p>Funds should arrive within 3-5 business days.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPaymentReceivedAsync(string email, decimal amount, string description)
        {
            var subject = "Payment received";
            var body = $@"
                <p>You've received a payment of ${amount}.</p>
                <p>Description: {description}</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendNewMessageNotificationAsync(string email, string senderName)
        {
            var subject = $"New message from {senderName}";
            var body = $@"
                <p>{senderName} sent you a message on Arsmate.</p>
                <p>Login to read and reply!</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendNewContentNotificationAsync(string email, string creatorName)
        {
            var subject = $"{creatorName} posted new content";
            var body = $@"
                <p>{creatorName} just posted new exclusive content!</p>
                <p>Check it out on Arsmate.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAccountSuspendedAsync(string email, string reason)
        {
            var subject = "Account suspended";
            var body = $@"
                <p>Your account has been suspended.</p>
                <p>Reason: {reason}</p>
                <p>Please contact support for more information.</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAccountReactivatedAsync(string email)
        {
            var subject = "Account reactivated";
            var body = $@"
                <p>Good news! Your Arsmate account has been reactivated.</p>
                <p>Welcome back!</p>
            ";
            return await SendEmailAsync(email, subject, body);
        }
    }
}
