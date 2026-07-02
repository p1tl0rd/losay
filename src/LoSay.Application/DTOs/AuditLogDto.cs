using AutoMapper;
using LoSay.Application.Mapping;
using LoSay.Domain.Entities;

namespace LoSay.Application.DTOs
{
	public class AuditLogDto : IMapFrom<AuditLog>
	{
		public int Id { get; set; }
		public int No { get; set; }

		public string TableName { get; set; } = null!;
		public string Action { get; set; } = null!; // CREATE / UPDATE / DELETE

		public string PrimaryKey { get; set; } = null!;

		public string? OldValues { get; set; } // JSON
		public string? NewValues { get; set; } // JSON

		public string UserId { get; set; } = null!;
		public string FullName { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<AuditLog, AuditLogDto>().ReverseMap();
		}
	}
}
 
