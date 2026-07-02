using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Servies.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Servies.Implementations
{
	public class ItemService : IItemService
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ILogger<ItemService> _logger; // Thêm logger

		public ItemService(ApplicationDbContext dbContext, ILogger<ItemService> logger)
		{
			_dbContext = dbContext;
			_logger = logger;
		}
		public async Task<List<Item>> GetItemsAsync()
		{
			try
			{
				return await _dbContext.Items.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?y danh sách Item.");
				throw;
			}
		}

		public async Task<List<ItemDetail>> GetItemDetailsByItemIdAsync(int itemId)
		{
			try
			{
				return await _dbContext.ItemDetails
					.Where(d => d.ItemId == itemId)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi l?y chi ti?t Item cho ItemId: {itemId}.");
				throw;
			}
		}

		public async Task AddItemAsync(Item item)
		{
			try
			{
				_dbContext.Items.Add(item);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi thêm Item: {item.ItemName}.");
				throw;
			}
		}

		public async Task UpdateItemAsync(Item item)
		{
			try
			{
				_dbContext.Entry(item).State = EntityState.Modified;
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi c?p nh?t Item: {item.ItemName}.");
				throw;
			}
		}

		public async Task DeleteItemAsync(int id)
		{
			try
			{
				var itemToDelete = await _dbContext.Items.FindAsync(id);
				if (itemToDelete != null)
				{
					_dbContext.Items.Remove(itemToDelete);
					await _dbContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi xóa Item Id: {id}.");
				throw;
			}
		}

		public async Task AddItemDetailAsync(ItemDetail itemDetail)
		{
			try
			{
				_dbContext.ItemDetails.Add(itemDetail);
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi thêm ItemDetail cho ItemId: {itemDetail.ItemId}.");
				throw;
			}
		}

		public async Task UpdateItemDetailAsync(ItemDetail itemDetail)
		{
			try
			{
				_dbContext.Entry(itemDetail).State = EntityState.Modified;
				await _dbContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi c?p nh?t ItemDetail Id: {itemDetail.Id}.");
				throw;
			}
		}

		public async Task DeleteItemDetailAsync(int id)
		{
			try
			{
				var itemDetailToDelete = await _dbContext.ItemDetails.FindAsync(id);
				if (itemDetailToDelete != null)
				{
					_dbContext.ItemDetails.Remove(itemDetailToDelete);
					await _dbContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"L?i khi xóa ItemDetail Id: {id}.");
				throw;
			}
		}
	}
}
