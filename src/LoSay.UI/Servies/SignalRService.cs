using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace LoSay.Servies
{
	public class SignalRService : IAsyncDisposable
	{
		private readonly Dictionary<string, HubConnection> _hubConnections = new();

		public async Task<HubConnection> GetOrCreateHub(string hubName, NavigationManager navigationManager)
		{
			if (_hubConnections.ContainsKey(hubName) && _hubConnections[hubName].State == HubConnectionState.Connected)
			{
				return _hubConnections[hubName]; // Tr? v? k?t n?i n?u dã có
			}

			var hubUrl = $"{navigationManager.BaseUri}{hubName}"; // T?o URL cho Hub
			var hubConnection = new HubConnectionBuilder()
				.WithUrl(hubUrl)
				.WithAutomaticReconnect()
				.Build();

			await hubConnection.StartAsync();
			_hubConnections[hubName] = hubConnection;

			Console.WriteLine($"? K?t n?i {hubName} thành công!");
			return hubConnection;
		}
		public async Task DisconnectHub(string hubName)
		{
			if (_hubConnections.ContainsKey(hubName))
			{
				await _hubConnections[hubName].StopAsync();
				await _hubConnections[hubName].DisposeAsync();
				_hubConnections.Remove(hubName);
				Console.WriteLine($"? Ðã ng?t k?t n?i {hubName}!");
			}
		}

		public async ValueTask DisposeAsync()
		{
			foreach (var hub in _hubConnections.Values)
			{
				await hub.StopAsync();
				await hub.DisposeAsync();
			}
			_hubConnections.Clear();
			Console.WriteLine("? T?t c? các k?t n?i SignalR dã dóng!");
		}


	}
}
