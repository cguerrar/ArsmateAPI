// ====================================
// MessageService.cs
// ====================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Core.Interfaces;
using Arsmate.Core.DTOs;
using Arsmate.Core.DTOs.Message;
using Arsmate.Infrastructure.Data;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.DTOs.Message;

namespace Arsmate.Infrastructure.Services
{
    /// <summary>
    /// Servicio simplificado para gestión de mensajes
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly ArsmateDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            ArsmateDbContext context,
            INotificationService notificationService,
            ILogger<MessageService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto messageDto)
        {
            try
            {
                var sender = await _context.Users.FindAsync(senderId);
                var recipient = await _context.Users.FindAsync(messageDto.RecipientId);

                if (sender == null || recipient == null)
                    throw new ArgumentException("Invalid sender or recipient");

                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    SenderId = senderId,
                    RecipientId = messageDto.RecipientId,
                    Content = messageDto.Content,
                    IsPaid = messageDto.Price > 0,
                    Price = messageDto.Price,
                    TipAmount = messageDto.TipAmount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Crear notificación
                await _notificationService.CreateNotificationAsync(
                    recipient.Id,
                    NotificationType.NewMessage,
                    "New Message",
                    $"You have a new message from {sender.Username}",
                    senderId
                );

                return new MessageDto
                {
                    Id = message.Id,
                    SenderId = message.SenderId,
                    RecipientId = message.RecipientId,
                    Content = message.Content,
                    IsPaid = message.IsPaid,
                    Price = message.Price,
                    IsRead = message.IsRead,
                    CreatedAt = message.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw;
            }
        }

        public async Task<PaginatedResultDto<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 50)
        {
            var query = _context.Messages
                .Where(m => (m.SenderId == userId && m.RecipientId == otherUserId) ||
                           (m.SenderId == otherUserId && m.RecipientId == userId))
                .OrderByDescending(m => m.CreatedAt);

            var totalItems = await query.CountAsync();
            var skip = (page - 1) * pageSize;

            var messages = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    RecipientId = m.RecipientId,
                    Content = m.Content,
                    IsPaid = m.IsPaid,
                    Price = m.Price,
                    IsRead = m.IsRead,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return new PaginatedResultDto<MessageDto>
            {
                Items = messages,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<PaginatedResultDto<ConversationDto>> GetUserConversationsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var conversations = await _context.Messages
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault(),
                    UnreadCount = g.Count(m => m.RecipientId == userId && !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessage.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await _context.Messages
                .Where(m => m.SenderId == userId || m.RecipientId == userId)
                .Select(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
                .Distinct()
                .CountAsync();

            var userIds = conversations.Select(c => c.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var items = conversations.Select(c => new ConversationDto
            {
                UserId = c.UserId,
                Username = users[c.UserId].Username,
                DisplayName = users[c.UserId].DisplayName,
                AvatarUrl = users[c.UserId].ProfileImageUrl,
                LastMessage = c.LastMessage?.Content,
                LastMessageAt = (DateTime)(c.LastMessage?.CreatedAt),
                UnreadCount = c.UnreadCount,
                IsPinned = false,
                IsArchived = false
            }).ToList();

            return new PaginatedResultDto<ConversationDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<MessageDto> GetMessageByIdAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId &&
                    (m.SenderId == userId || m.RecipientId == userId));

            if (message == null)
                return null;

            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                RecipientId = message.RecipientId,
                Content = message.Content,
                IsPaid = message.IsPaid,
                Price = message.Price,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };
        }

        public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null || message.RecipientId != userId)
                return false;

            if (!message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                message.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> MarkConversationAsReadAsync(Guid userId, Guid otherUserId)
        {
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == otherUserId &&
                           m.RecipientId == userId &&
                           !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.ReadAt = now;
                    message.UpdatedAt = now;
                }
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteMessageAsync(Guid messageId, Guid userId)
        {
            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
                return false;

            if (message.SenderId != userId && message.RecipientId != userId)
                return false;

            if (message.SenderId == userId)
                message.DeletedBySender = true;
            else
                message.DeletedByRecipient = true;

            message.UpdatedAt = DateTime.UtcNow;

            if (message.DeletedBySender && message.DeletedByRecipient)
                _context.Messages.Remove(message);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId)
        {
            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.RecipientId == otherUserId) ||
                           (m.SenderId == otherUserId && m.RecipientId == userId))
                .ToListAsync();

            foreach (var message in messages)
            {
                if (message.SenderId == userId)
                    message.DeletedBySender = true;
                else
                    message.DeletedByRecipient = true;

                if (message.DeletedBySender && message.DeletedByRecipient)
                    _context.Messages.Remove(message);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Messages
                .CountAsync(m => m.RecipientId == userId && !m.IsRead);
        }

        public async Task<bool> PurchaseMessageAsync(Guid messageId, Guid userId, string paymentMethod)
        {
            // TODO: Implementar lógica de compra
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> SetMessagePriceAsync(Guid userId, decimal? price)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.MessagePrice = price;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BlockUserMessagesAsync(Guid userId, Guid blockedUserId)
        {
            // TODO: Implementar tabla de bloqueos
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> UnblockUserMessagesAsync(Guid userId, Guid blockedUserId)
        {
            // TODO: Implementar tabla de bloqueos
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> PinConversationAsync(Guid userId, Guid otherUserId)
        {
            // TODO: Implementar tabla de conversaciones pineadas
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> UnpinConversationAsync(Guid userId, Guid otherUserId)
        {
            // TODO: Implementar tabla de conversaciones pineadas
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> ArchiveConversationAsync(Guid userId, Guid otherUserId)
        {
            // TODO: Implementar tabla de conversaciones archivadas
            await Task.Delay(10);
            return true;
        }

        public async Task<bool> UnarchiveConversationAsync(Guid userId, Guid otherUserId)
        {
            // TODO: Implementar tabla de conversaciones archivadas
            await Task.Delay(10);
            return true;
        }
    }
}