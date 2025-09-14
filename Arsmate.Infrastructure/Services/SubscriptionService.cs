using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.Subscription;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;

namespace Arsmate.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ArsmateDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            ArsmateDbContext context,
            IPaymentService paymentService,
            INotificationService notificationService,
            ILogger<SubscriptionService> logger)
        {
            _context = context;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(Guid subscriberId, CreateSubscriptionDto createDto)
        {
            // Check if already subscribed
            var existing = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId &&
                                          s.CreatorId == createDto.CreatorId &&
                                          s.IsActive);

            if (existing != null)
            {
                throw new InvalidOperationException("Already subscribed to this creator");
            }

            var creator = await _context.Users.FindAsync(createDto.CreatorId);
            if (creator == null || !creator.IsCreator)
            {
                throw new KeyNotFoundException("Creator not found");
            }

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                SubscriberId = subscriberId,
                CreatorId = createDto.CreatorId,
                StartDate = DateTime.UtcNow,
                NextBillingDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                AutoRenew = createDto.AutoRenew,
                PriceAtSubscription = creator.SubscriptionPrice ?? 0,
                Currency = creator.Currency ?? "USD",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Subscriptions.AddAsync(subscription);

            // Update creator's subscriber count
            creator.FollowersCount++;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Subscription created: {subscription.Id}");

            return await GetSubscriptionAsync(subscription.Id);
        }

        public async Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Creator)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null) return null;

            return new SubscriptionDto
            {
                Id = subscription.Id,
                SubscriberId = subscription.SubscriberId,
                CreatorId = subscription.CreatorId,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextBillingDate = subscription.NextBillingDate,
                IsActive = subscription.IsActive,
                AutoRenew = subscription.AutoRenew,
                PriceAtSubscription = subscription.PriceAtSubscription,
                Currency = subscription.Currency,
                DiscountPercentage = subscription.DiscountPercentage,
                FreeDays = subscription.FreeDays,
                CreatedAt = subscription.CreatedAt
            };
        }

        public async Task<SubscriptionDto> GetSubscriptionBetweenUsersAsync(Guid subscriberId, Guid creatorId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Creator)
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId &&
                                          s.CreatorId == creatorId &&
                                          s.IsActive);

            if (subscription == null) return null;

            return await GetSubscriptionAsync(subscription.Id);
        }

        public async Task<bool> UpdateSubscriptionAsync(Guid subscriptionId, Guid userId, UpdateSubscriptionDto updateDto)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || subscription.SubscriberId != userId) return false;

            if (updateDto.AutoRenew.HasValue)
            {
                subscription.AutoRenew = updateDto.AutoRenew.Value;
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, Guid userId, CancelSubscriptionDto cancelDto)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || subscription.SubscriberId != userId) return false;

            if (cancelDto.CancelImmediately)
            {
                subscription.IsActive = false;
                subscription.EndDate = DateTime.UtcNow;
            }
            else
            {
                subscription.AutoRenew = false;
                subscription.EndDate = subscription.NextBillingDate;
            }

            subscription.CancellationReason = cancelDto.Reason;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.UpdatedAt = DateTime.UtcNow;

            // Update creator's subscriber count
            var creator = await _context.Users.FindAsync(subscription.CreatorId);
            if (creator != null)
            {
                creator.FollowersCount = Math.Max(0, creator.FollowersCount - 1);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Subscription cancelled: {subscriptionId}");
            return true;
        }

        public async Task<bool> ReactivateSubscriptionAsync(Guid subscriptionId, Guid userId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || subscription.SubscriberId != userId) return false;

            subscription.IsActive = true;
            subscription.AutoRenew = true;
            subscription.EndDate = null;
            subscription.CancelledAt = null;
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Subscription reactivated: {subscriptionId}");
            return true;
        }

        public async Task<PaginatedResultDto<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var query = _context.Subscriptions
                .Include(s => s.Creator)
                .Where(s => s.SubscriberId == userId && s.IsActive)
                .OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();

            var subscriptions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    SubscriberId = s.SubscriberId,
                    CreatorId = s.CreatorId,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    NextBillingDate = s.NextBillingDate,
                    IsActive = s.IsActive,
                    AutoRenew = s.AutoRenew,
                    PriceAtSubscription = s.PriceAtSubscription,
                    Currency = s.Currency,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<SubscriptionDto>(subscriptions, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<SubscriptionDto>> GetCreatorSubscribersAsync(Guid creatorId, int page = 1, int pageSize = 20)
        {
            var query = _context.Subscriptions
                .Include(s => s.Subscriber)
                .Where(s => s.CreatorId == creatorId && s.IsActive)
                .OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();

            var subscriptions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    SubscriberId = s.SubscriberId,
                    CreatorId = s.CreatorId,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    NextBillingDate = s.NextBillingDate,
                    IsActive = s.IsActive,
                    AutoRenew = s.AutoRenew,
                    PriceAtSubscription = s.PriceAtSubscription,
                    Currency = s.Currency,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<SubscriptionDto>(subscriptions, totalCount, page, pageSize);
        }

        public async Task<bool> IsSubscribedAsync(Guid subscriberId, Guid creatorId)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.SubscriberId == subscriberId &&
                              s.CreatorId == creatorId &&
                              s.IsActive);
        }

        public async Task<SubscriptionStatsDto> GetCreatorStatsAsync(Guid creatorId)
        {
            var activeSubscribers = await _context.Subscriptions
                .CountAsync(s => s.CreatorId == creatorId && s.IsActive);

            var totalSubscribers = await _context.Subscriptions
                .CountAsync(s => s.CreatorId == creatorId);

            var monthlyRevenue = await _context.Subscriptions
                .Where(s => s.CreatorId == creatorId && s.IsActive)
                .SumAsync(s => s.PriceAtSubscription);

            return new SubscriptionStatsDto
            {
                TotalSubscribers = totalSubscribers,
                ActiveSubscribers = activeSubscribers,
                MonthlyRevenue = monthlyRevenue,
                AverageSubscriptionPrice = activeSubscribers > 0 ? monthlyRevenue / activeSubscribers : 0
            };
        }

        public async Task<bool> ProcessSubscriptionRenewalAsync(Guid subscriptionId)
        {
            var subscription = await _context.Subscriptions.FindAsync(subscriptionId);
            if (subscription == null || !subscription.IsActive || !subscription.AutoRenew)
                return false;

            // TODO: Process payment

            subscription.NextBillingDate = subscription.NextBillingDate?.AddMonths(1);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Subscription renewed: {subscriptionId}");
            return true;
        }

        public async Task<bool> ProcessExpiredSubscriptionsAsync()
        {
            var expired = await _context.Subscriptions
                .Where(s => s.IsActive && s.EndDate <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var subscription in expired)
            {
                subscription.IsActive = false;
                subscription.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Processed {expired.Count} expired subscriptions");
            return true;
        }

        public async Task<bool> SendExpirationRemindersAsync()
        {
            var expiringSoon = await _context.Subscriptions
                .Include(s => s.Subscriber)
                .Include(s => s.Creator)
                .Where(s => s.IsActive &&
                           s.NextBillingDate <= DateTime.UtcNow.AddDays(3))
                .ToListAsync();

            foreach (var subscription in expiringSoon)
            {
                // TODO: Send reminder email
                _logger.LogInformation($"Sending expiration reminder for subscription: {subscription.Id}");
            }

            return true;
        }

        public async Task<decimal> CalculateSubscriptionPriceAsync(Guid creatorId, string promoCode = null)
        {
            var creator = await _context.Users.FindAsync(creatorId);
            if (creator == null) return 0;

            var price = creator.SubscriptionPrice ?? 0;

            // TODO: Apply promo code discount if valid

            return price;
        }
    }
}

