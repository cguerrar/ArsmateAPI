

// ========================================
// Archivo: Arsmate.API/Services/SignalRNotificationService.cs
// ========================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Arsmate.API.Hubs;
using Arsmate.Core.Interfaces;

namespace Arsmate.API.Services
{
    /// <summary>
    /// Implementación de notificaciones en tiempo real usando SignalR
    /// Esta clase SÍ puede estar en el proyecto API porque tiene acceso a SignalR
    /// </summary>
    public class SignalRNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(string userId, string method, object data)
        {
            await _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync(method, data);
        }

        public async Task SendToGroupAsync(string groupName, string method, object data)
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(method, data);
        }

        public async Task SendToAllAsync(string method, object data)
        {
            await _hubContext.Clients
                .All
                .SendAsync(method, data);
        }
    }
}