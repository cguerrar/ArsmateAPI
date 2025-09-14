// ========================================
// Archivo: Arsmate.Core/Interfaces/IRealtimeNotificationService.cs
// ========================================

using System.Threading.Tasks;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para servicios de notificaciones en tiempo real
    /// </summary>
    public interface IRealtimeNotificationService
    {
        /// <summary>
        /// Envía una notificación a un usuario específico
        /// </summary>
        Task SendToUserAsync(string userId, string method, object data);

        /// <summary>
        /// Envía una notificación a un grupo
        /// </summary>
        Task SendToGroupAsync(string groupName, string method, object data);

        /// <summary>
        /// Envía una notificación a todos los usuarios conectados
        /// </summary>
        Task SendToAllAsync(string method, object data);
    }
}