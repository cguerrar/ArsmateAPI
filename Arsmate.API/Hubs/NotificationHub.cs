using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Arsmate.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation($"User {userId} connected to notification hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation($"User {userId} disconnected from notification hub");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, object notification)
        {
            await Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", notification);
        }
    }

    /// <summary>
    /// SignalR Hub for real-time chat
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            _logger.LogInformation($"User joined conversation: {conversationId}");
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
            _logger.LogInformation($"User left conversation: {conversationId}");
        }

        public async Task SendMessage(string conversationId, object message)
        {
            await Clients.Group($"conversation-{conversationId}").SendAsync("ReceiveMessage", message);
        }

        public async Task TypingIndicator(string conversationId, bool isTyping)
        {
            await Clients.OthersInGroup($"conversation-{conversationId}")
                .SendAsync("UserTyping", Context.UserIdentifier, isTyping);
        }
    }
}