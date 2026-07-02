using LoSay.Data.Entities;
using LoSay.Model;
using Microsoft.AspNetCore.SignalR;

namespace LoSay.Servies
{
	public class MachineHub : Hub
	{
		public async Task SendMachineStatus(ServiceResponse<List<Machine>> serviceResponse)
		{

			await Clients.All.SendAsync("ReceiveMachineStatus", serviceResponse);
		}

	}

}

