using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Arsmate.Core.DTOs.Common;
using Arsmate.Core.Entities;
using Arsmate.Core.Enums;
using Arsmate.Core.Interfaces;
using Arsmate.Infrastructure.Data;

namespace Arsmate.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ArsmateDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            ArsmateDbContext context,
            INotificationService notificationService,
            ILogger<ReportService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Report> CreateReportAsync(
            Guid reporterId,
            ReportType type,
            ReportReason reason,
            string description,
            Guid? targetId)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                ReporterId = reporterId,
                Type = type,
                Reason = reason,
                Description = description,
                Status = ReportStatus.Pending,
                Priority = GetPriorityForReason(reason),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set the appropriate target based on type
            switch (type)
            {
                case ReportType.User:
                    report.ReportedUserId = targetId;
                    break;
                case ReportType.Post:
                    report.ReportedPostId = targetId;
                    break;
                case ReportType.Comment:
                    report.ReportedCommentId = targetId;
                    break;
                case ReportType.Message:
                    report.ReportedMessageId = targetId;
                    break;
            }

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Report created: {report.Id} for {type} with reason {reason}");

            // Notify moderators for high priority reports
            if (report.Priority == ReportPriority.Critical || report.Priority == ReportPriority.High)
            {
                await NotifyModeratorsAsync(report);
            }

            return report;
        }

        public async Task<Report> GetReportByIdAsync(Guid reportId)
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedPost)
                .Include(r => r.ReviewedByUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<PaginatedResultDto<Report>> GetReportsAsync(
            ReportStatus? status = null,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            query = query.OrderByDescending(r => r.Priority)
                        .ThenByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResultDto<Report>(reports, totalCount, page, pageSize);
        }

        public async Task<PaginatedResultDto<Report>> GetUserReportsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Reports
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reports = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResultDto<Report>(reports, totalCount, page, pageSize);
        }

        public async Task<bool> UpdateReportStatusAsync(
            Guid reportId,
            ReportStatus status,
            Guid reviewerId,
            string notes)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return false;

            report.Status = status;
            report.ReviewedByUserId = reviewerId;
            report.ReviewedAt = DateTime.UtcNow;
            report.ModeratorNotes = notes;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Report {reportId} status updated to {status} by {reviewerId}");
            return true;
        }

        public async Task<bool> ResolveReportAsync(
            Guid reportId,
            Guid reviewerId,
            string actionTaken)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return false;

            report.Status = ReportStatus.Resolved;
            report.ReviewedByUserId = reviewerId;
            report.ReviewedAt = DateTime.UtcNow;
            report.ResolvedAt = DateTime.UtcNow;
            report.ActionTaken = actionTaken;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify reporter if configured
            if (report.ReporterNotified)
            {
                await _notificationService.CreateNotificationAsync(
                    report.ReporterId,
                    NotificationType.SystemAlert,
                    "Report Resolved",
                    $"Your report has been reviewed and resolved. Action taken: {actionTaken}");
            }

            _logger.LogInformation($"Report {reportId} resolved by {reviewerId}");
            return true;
        }

        public async Task<bool> DismissReportAsync(
            Guid reportId,
            Guid reviewerId,
            string reason)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return false;

            report.Status = ReportStatus.Dismissed;
            report.ReviewedByUserId = reviewerId;
            report.ReviewedAt = DateTime.UtcNow;
            report.ResolvedAt = DateTime.UtcNow;
            report.ModeratorNotes = reason;
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Report {reportId} dismissed by {reviewerId}");
            return true;
        }

        public async Task<bool> EscalateReportAsync(
            Guid reportId,
            Guid reviewerId,
            string reason)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return false;

            report.Status = ReportStatus.Escalated;
            report.Priority = ReportPriority.Critical;
            report.ReviewedByUserId = reviewerId;
            report.ReviewedAt = DateTime.UtcNow;
            report.ModeratorNotes = $"Escalated: {reason}";
            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify senior moderators/admins
            await NotifyAdminsAsync(report, reason);

            _logger.LogInformation($"Report {reportId} escalated by {reviewerId}");
            return true;
        }

        public async Task<int> GetPendingReportsCountAsync()
        {
            return await _context.Reports
                .CountAsync(r => r.Status == ReportStatus.Pending);
        }

        public async Task<bool> CheckUserReportedAsync(Guid userId)
        {
            return await _context.Reports
                .AnyAsync(r => r.ReportedUserId == userId &&
                              r.Status != ReportStatus.Dismissed);
        }

        public async Task<bool> CheckContentReportedAsync(Guid contentId, ReportType type)
        {
            return type switch
            {
                ReportType.Post => await _context.Reports
                    .AnyAsync(r => r.ReportedPostId == contentId &&
                                  r.Status != ReportStatus.Dismissed),
                ReportType.Comment => await _context.Reports
                    .AnyAsync(r => r.ReportedCommentId == contentId &&
                                  r.Status != ReportStatus.Dismissed),
                ReportType.Message => await _context.Reports
                    .AnyAsync(r => r.ReportedMessageId == contentId &&
                                  r.Status != ReportStatus.Dismissed),
                _ => false
            };
        }

        private ReportPriority GetPriorityForReason(ReportReason reason)
        {
            return reason switch
            {
                ReportReason.Underage => ReportPriority.Critical,
                ReportReason.SelfHarm => ReportPriority.Critical,
                ReportReason.Terrorism => ReportPriority.Critical,
                ReportReason.IllegalContent => ReportPriority.Critical,
                ReportReason.Violence => ReportPriority.High,
                ReportReason.HateSpeech => ReportPriority.High,
                ReportReason.HarassmentOrBullying => ReportPriority.High,
                ReportReason.Nudity => ReportPriority.Normal,
                ReportReason.Spam => ReportPriority.Low,
                _ => ReportPriority.Normal
            };
        }

        private async Task NotifyModeratorsAsync(Report report)
        {
            // TODO: Get moderator user IDs and send notifications
            _logger.LogInformation($"Notifying moderators about high priority report: {report.Id}");
            await Task.CompletedTask;
        }

        private async Task NotifyAdminsAsync(Report report, string reason)
        {
            // TODO: Get admin user IDs and send notifications
            _logger.LogInformation($"Notifying admins about escalated report: {report.Id}");
            await Task.CompletedTask;
        }
    }
}