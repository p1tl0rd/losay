using LoSay.Application.DTOs.Auth;

namespace LoSay.Application.Services
{
	public interface IAuthService
	{
		Task<string> LoginAsync(UserAuthDto userAuthDto);
		Task<bool> UserExistsAsync(string email);
	}
}
