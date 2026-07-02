using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class ItemRepository : RepositoryBase<Item, int>, IItemRepository
	{
		public ItemRepository(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork) : base(context, unitOfWork)
		{
		}

		public Task<int> CreatetemAsync(Item item) => CreateAsync(item);

		public Task DeleteItemAsync(int id)
		{
			var entity = FindByCondition(p => p.Id.Equals(id)).SingleOrDefault();
			DeleteAsync(entity);
			return Task.CompletedTask;
		}

		public async Task<IEnumerable<Item>> GetItemsAsync() => await FindAll().ToListAsync();

		public Task UpdateItemAsync(Item item) => UpdateAsync(item);
	}
}
