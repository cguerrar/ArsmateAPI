using System.Threading.Tasks;
using System.Collections.Generic;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de email
    /// </summary>
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true);
        Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string email, string username);
        Task<bool> SendSubscriptionConfirmationAsync(string email, string creatorName, decimal amount);
        Task<bool> SendSubscriptionCancelledAsync(string email, string creatorName);
        Task<bool> SendSubscriptionExpiringAsync(string email, string creatorName, int daysRemaining);
        Task<bool> SendNewSubscriberNotificationAsync(string creatorEmail, string subscriberName);
        Task<bool> SendWithdrawalProcessedAsync(string email, decimal amount, string method);
        Task<bool> SendPaymentReceivedAsync(string email, decimal amount, string description);
        Task<bool> SendNewMessageNotificationAsync(string email, string senderName);
        Task<bool> SendNewContentNotificationAsync(string email, string creatorName);
        Task<bool> SendAccountSuspendedAsync(string email, string reason);
        Task<bool> SendAccountReactivatedAsync(string email);
    }
}