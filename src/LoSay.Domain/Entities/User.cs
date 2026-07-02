using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LoSay.Domain.Entities
{
	public class User : IdentityUser
	{
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
