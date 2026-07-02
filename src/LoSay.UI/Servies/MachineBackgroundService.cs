using LoSay.Data.Contexts;
using LoSay.Data.Entities;
using LoSay.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Servies
{
	public class MachineBackgroundService : BackgroundService
	{
		private readonly IHubContext<MachineHub> _hubContext;
		private readonly ApplicationDbContext _dbContext;
		private ServiceResponse<List<Machine>> serviceResponse;
		public MachineBackgroundService(ApplicationDbContext applicationDbContext, IHubContext<MachineHub> hubContext)
		{
			_hubContext = hubContext;
			_dbContext = applicationDbContext;
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				var query = await _dbContext.Machines.ToListAsync();
			
				serviceResponse = new ServiceResponse<List<Machine>>() { Data = query, IsSuccess = true, Message = "OK" };

				await _hubContext.Clients.All.SendAsync("ReceiveMachineStatus", serviceResponse);

				await Task.Delay(2000, stoppingToken); // Ki?m tra m?i 2 giây
			}
		}
	}
}
