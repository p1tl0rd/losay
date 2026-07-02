using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoSay.Data.Entities
{
	[Table("AuditLog")]
	public class AuditLog
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string TableName { get; set; } = null!;
		public string Action { get; set; } = null!;
		public string PrimaryKey { get; set; } = null!;
		public string? OldValues { get; set; }
		public string? NewValues { get; set; }
		public string UserId { get; set; } = null!;
		public DateTimeOffset CreatedDate { get; set; }
	}
}
