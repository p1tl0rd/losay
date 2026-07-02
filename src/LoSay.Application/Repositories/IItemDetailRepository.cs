using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface IItemDetailRepository : IRepositoryBase<ItemDetail, int>
	{
		Task<IEnumerable<ItemDetail>> GetItemDetailsByItemIdAsync(int itemId);
		Task<ItemDetail> GetItemDetailByItemIdAsync(int itemId);
		// ItemDetail CRUD
		Task<int> CreateItemDetailAsync(ItemDetail itemDetail);
		Task UpdateItemDetailAsync(ItemDetail itemDetail);
		Task DeleteItemDetailAsync(int id);
	}
}
