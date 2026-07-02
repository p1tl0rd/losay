using System.Linq.Expressions;
using System.Text.Json;
using LoSay.Application.Services;
using LoSay.Domain.Common.Interfaces;
using LoSay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Data.Contexts
{
	public class ApplicationDbContext : DbContext
	{
		//private readonly IUserService _userService;
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				if (typeof(ISoftDeleteTracking).IsAssignableFrom(entityType.ClrType))
				{
					var parameter = Expression.Parameter(entityType.ClrType, "e");
					var prop = Expression.Property(parameter, nameof(ISoftDeleteTracking.IsDeleted));
					var body = Expression.Equal(prop, Expression.Constant(false));
					var lambda = Expression.Lambda(body, parameter);

					modelBuilder.Entity(entityType.ClrType)
						.HasQueryFilter(lambda);
				}
			}

		}
		//public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		//{
		//	var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

		//	var userId = await _userService.GetUserLogin();
		//	var auditLogs = new List<AuditLog>();

		//	foreach (var item in modified)
		//	{
		//		switch (item.State)
		//		{
		//			case EntityState.Added:
		//				if (item.Entity is IDateTracking addedEntity)
		//				{
		//					addedEntity.CreatedDate = DateTime.UtcNow;
		//					item.State = EntityState.Added;
		//				}
		//				if (item.Entity is IUserTracking addedEntityUser)
		//				{
		//					addedEntityUser.CreatedBy = userId;
		//					item.State = EntityState.Added;
		//				}
		//				break;
		//			case EntityState.Modified:
		//				Entry(item.Entity).Property("Id").IsModified = false;
		//				if (item.Entity is IDateTracking modifiedEntity)
		//				{
		//					modifiedEntity.LastModifiedDate = DateTime.UtcNow;
		//					item.State = EntityState.Modified;
		//				}
		//				if (item.Entity is IUserTracking modifiedEntityUser)
		//				{
		//					modifiedEntityUser.LastModifiedBy = userId;
		//					item.State = EntityState.Modified;
		//				}
		//				break;
		//			case EntityState.Deleted:

		//				break;

		//			default:
		//				break;
		//		}
		//	}


		//	return await base.SaveChangesAsync(cancellationToken);
		//}
		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			//var userId = await _userService.GetUserLogin();
			var userId = "";
			var auditLogs = new List<AuditLog>();

			// ===== 1. Set audit fields + prepare audit =====
			foreach (var entry in ChangeTracker.Entries())
			{
				if (entry.State == EntityState.Unchanged) continue;

				if (entry.Entity is IDateTracking dateTracking)
				{
					if (entry.State == EntityState.Added)
						dateTracking.CreatedDate = DateTime.UtcNow;
					else if (entry.State == EntityState.Modified)
						dateTracking.LastModifiedDate = DateTime.UtcNow;
				}

				if (entry.Entity is IUserTracking userTracking)
				{
					if (entry.State == EntityState.Added)
						userTracking.CreatedBy = userId;
					else if (entry.State == EntityState.Modified)
						userTracking.LastModifiedBy = userId;
				}

				// ===== Audit =====
				if (entry.Entity is IAuditable)
				{
					var audit = new AuditLog
					{
						TableName = entry.Metadata.GetTableName()!,
						Action = entry.State.ToString().ToUpper(),
						UserId = userId,
						CreatedDate = DateTimeOffset.UtcNow,
						PrimaryKey = JsonSerializer.Serialize(
							entry.Properties
								.Where(p => p.Metadata.IsPrimaryKey())
								.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue))
					};

					if (entry.State == EntityState.Modified)
					{
						audit.OldValues = JsonSerializer.Serialize(
							entry.Properties
								.Where(p => p.IsModified)
								.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue)
						);

						audit.NewValues = JsonSerializer.Serialize(
							entry.Properties
								.Where(p => p.IsModified)
								.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue)
						);
					}
					else if (entry.State == EntityState.Added)
					{
						audit.NewValues = JsonSerializer.Serialize(
							entry.Properties.ToDictionary(
								p => p.Metadata.Name,
								p => p.CurrentValue)
						);
					}
					else if (entry.State == EntityState.Deleted)
					{
						audit.OldValues = JsonSerializer.Serialize(
							entry.Properties.ToDictionary(
								p => p.Metadata.Name,
								p => p.OriginalValue)
						);
					}

					auditLogs.Add(audit);
				}
			}

			// ===== 2. Save entity =====
			var result = await base.SaveChangesAsync(cancellationToken);

			// ===== 3. Save audit =====
			if (auditLogs.Count > 0 && userId != null)
			{
				await Set<AuditLog>().AddRangeAsync(auditLogs);
				await base.SaveChangesAsync(cancellationToken);
			}

			return result;
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


