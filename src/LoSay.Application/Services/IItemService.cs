using LoSay.Application.DTOs;
using LoSay.Domain.Entities;

namespace LoSay.Application.Services
{
	public interface IItemService
	{
		Task<List<ItemDto>> GetItemsAsync();
		Task<List<ItemDetailDto>> GetItemDetailsByItemIdAsync(int itemId);
		// Item CRUD
		Task<int> CreateItemAsync(ItemDto item);
		Task UpdateItemAsync(ItemDto item);
		Task DeleteItemAsync(int id);

		// ItemDetail CRUD
		Task CreateItemDetailAsync(ItemDetailDto itemDetail);
		Task UpdateItemDetailAsync(ItemDetailDto itemDetail);
		Task DeleteItemDetailAsync(int id);
	}
}
