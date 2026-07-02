using LoSay.Data.Entities;
using LoSay.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Data.Contexts
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

		}
		public DbSet<Machine> Machines { get; set; }
		public DbSet<PLCData> PLCDatas { get; set; }
		public DbSet<Item> Items { get; set; }
		public DbSet<ItemDetail> ItemDetails { get; set; }
		public DbSet<LotState> LotStates { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<AuditLog> AuditLogs { get; set; }

	}
}


