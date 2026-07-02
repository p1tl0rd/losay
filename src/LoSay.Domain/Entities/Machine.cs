using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	[Table("Machine")]
	public class Machine : EntityAuditBase<int>
	{
		[MaxLength(10)]
		[Column(TypeName = "nvarchar(10)")]
		public string? MachineName { get; set; }
		[MaxLength(20)]
		[Column(TypeName = "nvarchar(20)")]
		public string? Address { get; set; }
		[MaxLength(20)]
		[Column(TypeName = "nvarchar(20)")]
		public string? LotRun { get; set; }
		public bool? FinishLot { get; set; }
		public bool? Status { get; set; }

	}
}
