using LoSay.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace LoSay.Infrastructure.Services
{
	public class PLCHubService : Hub
	{
		public async Task JoinMachineGroup(int machineId)
		{
			string groupName = $"Machine_{machineId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
			Console.WriteLine($"Client {Context.ConnectionId} đa tham gia nhom {groupName}");
		}
		public async Task SendPlcDataToGroup(int machineId, PLCDataDto data)
		{
			string groupName = $"Machine_{machineId}";
			await Clients.Group(groupName).SendAsync("ReceivePlcValue", data);
			Console.WriteLine($"Sent data to group {groupName}");
		}


		public async Task JoinDashboardGroup()
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboard");
			Console.WriteLine($"Client {Context.ConnectionId} joined Dashboard group.");
		}
		public async Task SendAllMachineStatus(IEnumerable<MachineWithLotInfoDto> data)
		{
		//	await Clients.All.SendAsync("SendAllMachineStatus", data);
			await Clients.Group("Dashboard").SendAsync("ReceiveDashboardData", data);
		}

	}
}
