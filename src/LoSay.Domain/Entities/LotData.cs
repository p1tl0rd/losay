using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	[Table("LotState")]
	public class LotState : EntityAuditBase<int>
	{
		public int MachineId { get; set; }
		[MaxLength(30)]
		[Column(TypeName = "nvarchar(30)")]
		public string? LotNo { get; set; }
		public DateTime? StartTime { get; set; }

		public DateTime? EndTime { get; set; }
		public bool IsRunning { get; set; } = false; // Mặc định là false

	}
}
