using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoSay.Data.Entities
{
	public class User
	{
		[Key]
		public string Id { get; set; } = Guid.NewGuid().ToString();
		[MaxLength(256)]
		public string? UserName { get; set; }
		[MaxLength(256)]
		public string? Email { get; set; }
		public int DivisionId { get; set; }
		public int SectionId { get; set; }
		public int ParentId { get; set; }
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string? FullName { get; set; }
		[MaxLength(50)]
		[Column(TypeName = "nvarchar(50)")]
		public string? Code { get; set; }
	}
}
