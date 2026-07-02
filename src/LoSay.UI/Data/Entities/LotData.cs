using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoSay.Data.Entities
{
	[Table("LotState")]
	public class LotState
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int MachineId { get; set; }
		[MaxLength(30)]
		[Column(TypeName = "nvarchar(30)")]
		public string? LotNo { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public bool IsRunning { get; set; } = false;
	}
}
