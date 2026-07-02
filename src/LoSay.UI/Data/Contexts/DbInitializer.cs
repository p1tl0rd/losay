using LoSay.Data.Entities;

namespace LoSay.Data.Contexts
{
	public class DbInitializer
	{
		private readonly ApplicationDbContext _context;
		public DbInitializer(ApplicationDbContext context)
		{
			_context = context;
		}
		public async Task Seed()
		{
			if (!_context.Items.Any())
			{
				_context.Items.AddRange(new List<Item>()
				{
					new Item {ItemCode = "HP24C25A2",ItemName = "SHAFT WELDED 1", Max  = Convert.ToDouble(120.5),Min = Convert.ToDouble(20.5),TimeSamping = 3600 ,Status = true},
					new Item {ItemCode = "HP25C26A3",ItemName = "SHAFT WELDED 2", Max  = Convert.ToDouble(120.5),Min = Convert.ToDouble(20.5),TimeSamping = 3600 ,Status = true},
					new Item {ItemCode = "HP26C27A4",ItemName = "SHAFT WELDED 3", Max  = Convert.ToDouble(120.5),Min = Convert.ToDouble(20.5),TimeSamping = 3600 ,Status = true},
					new Item {ItemCode = "HP27C28A5",ItemName = "SHAFT WELDED 4", Max  = Convert.ToDouble(120.5),Min = Convert.ToDouble(20.5),TimeSamping = 3600 ,Status = true},
				});
				await _context.SaveChangesAsync();
			}
			if (!_context.Machines.Any())
			{
				_context.Machines.AddRange(new List<Machine>()
				{
					new Machine { MachineName = "EB1-2737", Address = "110,111,112,113,114", Status = true },
					new Machine { MachineName = "EB1-2738", Address = "115,116,117,118,119", Status = true }
				});
				await _context.SaveChangesAsync();
			}
		}
	}
}
