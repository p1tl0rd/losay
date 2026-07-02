using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	public class AuditLog : EntityBase<int>
	{
		public string TableName { get; set; } = null!;
		public string Action { get; set; } = null!; // CREATE / UPDATE / DELETE

		public string PrimaryKey { get; set; } = null!;

		public string? OldValues { get; set; } // JSON
		public string? NewValues { get; set; } // JSON

		public string UserId { get; set; } = null!;
		public DateTimeOffset CreatedDate { get; set; }
	}
}
