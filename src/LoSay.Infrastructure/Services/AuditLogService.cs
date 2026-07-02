using LoSay.Application.DTOs;
using LoSay.Application.Services;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Services
{
	public class AuditLogService : IAuditLogService
	{
		private readonly ApplicationDbContext _context;

		public AuditLogService(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<List<AuditLogDto>> GetAllAsync()
		{
			var auditLog = await _context.AuditLogs
						.OrderByDescending(x => x.CreatedDate)
						.ToListAsync();

			var result = await( from l in _context.AuditLogs
						 join i in _context.Users on l.UserId equals i.Id
						 select new AuditLogDto
						 {
							 Id = l.Id,
							 TableName = l.TableName,
							 Action = l.Action,
							 PrimaryKey = l.PrimaryKey,
							 OldValues = l.OldValues,
							 NewValues = l.NewValues,
							 UserId = l.UserId,
							 FullName = i.FullName,
							 CreatedDate = l.CreatedDate
						 }).ToListAsync();
			return result;

		}
	}
}
