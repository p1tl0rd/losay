using LoSay.Data.Entities;

namespace LoSay.Servies.Interfaces
{
	public interface IItemService
	{
		Task<List<Item>> GetItemsAsync();
		Task<List<ItemDetail>> GetItemDetailsByItemIdAsync(int itemId);
		// Item CRUD
		Task AddItemAsync(Item item);
		Task UpdateItemAsync(Item item);
		Task DeleteItemAsync(int id);

		// ItemDetail CRUD
		Task AddItemDetailAsync(ItemDetail itemDetail);
		Task UpdateItemDetailAsync(ItemDetail itemDetail);
		Task DeleteItemDetailAsync(int id);
	}
}
