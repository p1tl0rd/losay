using LoSay.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Persistence;

public class UserDbContext : IdentityDbContext<User>
{
	public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
	{

	}
	public DbSet<User> Users { get; set; }
}
