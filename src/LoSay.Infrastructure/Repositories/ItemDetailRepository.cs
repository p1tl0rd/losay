using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class ItemDetailRepository : RepositoryBase<ItemDetail, int>, IItemDetailRepository
	{
		public ItemDetailRepository(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork) : base(context, unitOfWork)
		{
		}

		public Task<int> CreateItemDetailAsync(ItemDetail itemDetail) => CreateAsync(itemDetail);

		public Task DeleteItemDetailAsync(int id)
		{
			var entity = FindByCondition(p => p.Id.Equals(id)).SingleOrDefault();
			DeleteAsync(entity);
			return Task.CompletedTask;
		}

		public Task<ItemDetail> GetItemDetailByItemIdAsync(int itemId)
		=> FindByCondition(p => p.ItemId.Equals(itemId)).SingleOrDefaultAsync();

		public async Task<IEnumerable<ItemDetail>> GetItemDetailsByItemIdAsync(int itemId)
			=> await FindByCondition(p => p.ItemId.Equals(itemId)).ToListAsync();

		public Task UpdateItemDetailAsync(ItemDetail itemDetail) => UpdateAsync(itemDetail);
	}
}
