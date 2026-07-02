using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface IUserRepository
	{
		Task<User> CreateUser(User user);
		Task<User> GetUserById(string id);
		Task<User> GetUserByCode(string code);
	}
}
