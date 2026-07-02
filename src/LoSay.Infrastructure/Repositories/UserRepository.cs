using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly ApplicationDbContext _context;
		public UserRepository(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
		public async Task<User> CreateUser(User user)
		{
			if (GetUserById(user.Id) != null)
			{
				await _context.Users.AddAsync(user);
				await _context.SaveChangesAsync();
			}
			return user;
		}

		public async Task<User> GetUserByCode(string code)
			=> await _context.Users.Where(p => p.UserName.Equals(code)).SingleOrDefaultAsync();

		public async Task<User> GetUserById(string id)
			=> await _context.Users.FindAsync(id);
	}
}
