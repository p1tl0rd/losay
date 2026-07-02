using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories;

public interface IAuthRepository
{
	Task<int> RegisterAsync(User user, string password, int startUnitId);
	Task<string> LoginAsync(string code, string password);
	Task<bool> UserExistsAsync(string email);
	Task<User> GetUserByCode(string code);

}
