using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface IItemRepository : IRepositoryBase<Item, int>
	{
		Task<IEnumerable<Item>> GetItemsAsync();
		// Item CRUD
		Task<int> CreatetemAsync(Item item);
		Task UpdateItemAsync(Item item);
		Task DeleteItemAsync(int id);
	}
}
