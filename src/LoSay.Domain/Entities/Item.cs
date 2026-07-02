using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoSay.Domain.Common;

namespace LoSay.Domain.Entities
{
	[Table("Item")]
	public class Item : EntityAuditBase<int>
	{
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string ItemCode { get; set; }
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string ItemName { get; set; }
		public double Max { get; set; }
		public double Min { get; set; }
		public int TimeSamping { get; set; }
		public bool Status { get; set; }
	 
	}
}
