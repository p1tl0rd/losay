using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoSay.Data.Entities
{
	[Table("Item")]
	public class Item
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string ItemCode { get; set; } = null!;
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string ItemName { get; set; } = null!;
		public double Max { get; set; }
		public double Min { get; set; }
		public int TimeSamping { get; set; }
		public bool Status { get; set; }
	}
}
