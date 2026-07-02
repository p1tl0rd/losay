using LoSay.Data.Contexts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace LoSay.Servies
{
	public class PlcHub : Hub
	{
		//private readonly IServiceScopeFactory _serviceScopeFactory;
		//public PlcHub(IServiceScopeFactory serviceScopeFactory)
		//{
		//	_serviceScopeFactory = serviceScopeFactory;

		//}

		//public async Task GetLatestPlcData(int machineId)
		//{
		//	while (true)
		//	{
		//		using var scope = _serviceScopeFactory.CreateScope();
		//		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		//		var latestData = await dbContext.PLCDatas
		//			.Where(p => p.MachineId == machineId) // ? L?c theo MachineId
		//			.OrderByDescending(p => p.Timestamp)
		//			.FirstOrDefaultAsync();

		//		if (latestData != null)
		//		{
		//			Random rnd = new Random();
		//			//await Clients.All.SendAsync("ReceivePlcValue",
		//				//latestData.MachineId, latestData.Value1, latestData.Value2, latestData.Value3, latestData.Value4, latestData.Value5);
		//			//	latestData.MachineId, latestData.LotNo, rnd.Next(120), rnd.Next(120), rnd.Next(120), rnd.Next(120), rnd.Next(120));
		//			await Clients.Group($"Machine_{latestData.MachineId}").SendAsync("ReceivePlcValue",
		//				latestData.MachineId, latestData.LotNo, rnd.Next(120), rnd.Next(120), rnd.Next(120), rnd.Next(120), rnd.Next(120));

		//		}
		//		await Task.Delay(2000); 
		//	}

		//}
		public async Task JoinMachineGroup(int machineId)
		{
			string groupName = $"Machine_{machineId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			Console.WriteLine($"?? Client {Context.ConnectionId} dã tham gia nhóm {groupName}");
		}

	}
}
