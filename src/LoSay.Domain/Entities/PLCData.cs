using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	[Table("PLCData")]
	public class PLCData : EntityAuditBase<int>
	{
		public int MachineId { get; set; }
		public int ItemCodeId { get; set; }
		[MaxLength(30)]
		[Column(TypeName = "nvarchar(30)")]
		public string? LotNo { get; set; }
		public double? Value1 { get; set; }
		public double? Value2 { get; set; }
		public double? Value3 { get; set; }
		public double? Value4 { get; set; }
		public double? Value5 { get; set; }
		public DateTime? Timestamp { get; set; }
		public bool Status { get; set; }
	}
}
