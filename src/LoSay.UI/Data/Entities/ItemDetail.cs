using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoSay.Data.Entities
{
	[Table("ItemDetail")]
	public class ItemDetail
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int? ItemId { get; set; }
		public string? ClassName { get; set; }
		public double Temperature { get; set; }
		public double TimeDrying { get; set; }
		public bool Status { get; set; }
	}
}
