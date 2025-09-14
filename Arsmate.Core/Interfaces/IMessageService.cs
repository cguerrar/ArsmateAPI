using System;
using System.Threading.Tasks;
using Arsmate.Core.DTOs.Message;
using Arsmate.Core.DTOs.Common;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de mensajería
    /// </summary>
    public interface IMessageService
    {
        Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto sendMessageDto);
        Task<MessageDto> GetMessageByIdAsync(Guid messageId, Guid userId);
        Task<PaginatedResultDto<MessageDto>> GetConversationAsync(Guid userId, Guid otherUserId, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<ConversationDto>> GetUserConversationsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId);
        Task<bool> MarkConversationAsReadAsync(Guid userId, Guid otherUserId);
        Task<bool> DeleteMessageAsync(Guid messageId, Guid userId);
        Task<bool> DeleteConversationAsync(Guid userId, Guid otherUserId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> PurchaseMessageAsync(Guid messageId, Guid userId, string paymentMethodId);
        Task<bool> BlockUserMessagesAsync(Guid userId, Guid blockedUserId);
        Task<bool> UnblockUserMessagesAsync(Guid userId, Guid blockedUserId);
        Task<bool> SetMessagePriceAsync(Guid userId, decimal? price);
        Task<bool> ArchiveConversationAsync(Guid userId, Guid otherUserId);
        Task<bool> UnarchiveConversationAsync(Guid userId, Guid otherUserId);
        Task<bool> PinConversationAsync(Guid userId, Guid otherUserId);
        Task<bool> UnpinConversationAsync(Guid userId, Guid otherUserId);
    }
}