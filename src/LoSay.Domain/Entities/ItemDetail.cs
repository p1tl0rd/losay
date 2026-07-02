using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	[Table("ItemDetail")]
	public class ItemDetail : EntityAuditBase<int>
	{
		public int? ItemId { get; set; }
		public string? ClassName { get; set; }
		public double Temperature { get; set; }
		public double TimeDrying { get; set; }
		public bool Status { get; set; }
	}
}
