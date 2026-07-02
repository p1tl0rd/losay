using LoSay.Application.DTOs;

namespace LoSay.Application.Services
{
	public interface IUserService
	{
		Task<string> GetUserLogin();
	}
}
