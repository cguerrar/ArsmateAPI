using System;
using System.Threading.Tasks;
using Arsmate.Core.DTOs.Subscription;
using Arsmate.Core.DTOs.Common;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de suscripciones
    /// </summary>
    public interface ISubscriptionService
    {
        Task<SubscriptionDto> CreateSubscriptionAsync(Guid subscriberId, CreateSubscriptionDto createDto);
        Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId);
        Task<SubscriptionDto> GetSubscriptionBetweenUsersAsync(Guid subscriberId, Guid creatorId);
        Task<bool> UpdateSubscriptionAsync(Guid subscriptionId, Guid userId, UpdateSubscriptionDto updateDto);
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId, CancelSubscriptionDto cancelDto);
        Task<bool> ReactivateSubscriptionAsync(Guid subscriptionId, Guid userId);
        Task<PaginatedResultDto<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<SubscriptionDto>> GetCreatorSubscribersAsync(Guid creatorId, int page = 1, int pageSize = 20);
        Task<bool> IsSubscribedAsync(Guid subscriberId, Guid creatorId);
        Task<SubscriptionStatsDto> GetCreatorStatsAsync(Guid creatorId);
        Task<bool> ProcessSubscriptionRenewalAsync(Guid subscriptionId);
        Task<bool> ProcessExpiredSubscriptionsAsync();
        Task<bool> SendExpirationRemindersAsync();
        Task<decimal> CalculateSubscriptionPriceAsync(Guid creatorId, string promoCode = null);
    }
}