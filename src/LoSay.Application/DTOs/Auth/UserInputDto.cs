using System.ComponentModel.DataAnnotations;

namespace LoSay.Application.DTOs.Auth
{
	public class UserAuthDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }


		public string? Password { get; set; }


		public string? ConfirmPassword { get; set; }

		public string? Code { get; set; }
	}
}
