using System;
using System.Threading.Tasks;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;

namespace Arsmate.Core.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de reportes
    /// </summary>
    public interface IReportService
    {
        Task<Report> CreateReportAsync(Guid reporterId, ReportType type, ReportReason reason, string description, Guid? targetId);
        Task<Report> GetReportByIdAsync(Guid reportId);
        Task<PaginatedResultDto<Report>> GetReportsAsync(ReportStatus? status = null, int page = 1, int pageSize = 20);
        Task<PaginatedResultDto<Report>> GetUserReportsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> UpdateReportStatusAsync(Guid reportId, ReportStatus status, Guid reviewerId, string notes);
        Task<bool> ResolveReportAsync(Guid reportId, Guid reviewerId, string actionTaken);
        Task<bool> DismissReportAsync(Guid reportId, Guid reviewerId, string reason);
        Task<bool> EscalateReportAsync(Guid reportId, Guid reviewerId, string reason);
        Task<int> GetPendingReportsCountAsync();
        Task<bool> CheckUserReportedAsync(Guid userId);
        Task<bool> CheckContentReportedAsync(Guid contentId, ReportType type);
    }
}