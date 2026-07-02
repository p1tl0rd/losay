using LoSay.Application.DTOs;
using LoSay.Domain.Entities;

namespace LoSay.Application.Services;

public interface IAuditLogService
{
	Task<List<AuditLogDto>> GetAllAsync();
}
