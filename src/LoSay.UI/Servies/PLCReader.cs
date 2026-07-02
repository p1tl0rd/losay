using EasyModbus;

namespace LoSay.Servies
{
	public class PLCReader
	{
		private readonly string _ip;
		private readonly int _port;
		private ModbusClient _modbusClient;

		public PLCReader(string ip, int port )
		{
			_ip = ip;
			_port = port;
			_modbusClient = new ModbusClient { IPAddress = _ip, Port = _port, ConnectionTimeout = 20000 };
		}

		public async Task<bool> Connect()
		{
			bool isConntected = false;
			try
			{
			//	_modbusClient.Connect();
				await Task.Run(() => _modbusClient.Connect());
				isConntected = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"? Connect error: {ex.Message}");
			}
			return isConntected;
		}

		public async Task<int> ReadRegisterAsync(int address)
		{
			if (_modbusClient == null)
				throw new InvalidOperationException("ModbusClient chua du?c k?t n?i.");

			int[] registers = await Task.Run(() => _modbusClient.ReadHoldingRegisters(address, 1));
			return registers[0];
		}
		/// <summary>
		/// Ð?c d? li?u t?i ô nh? thanh ghi d?u tiên
		/// </summary>
		/// <param name="address"></param>
		/// <param name="quantity"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public async Task<int[]> ReadListRegisterAsync(int[] address, int quantity)
		{
			if (_modbusClient == null)
				throw new InvalidOperationException("ModbusClient chua du?c k?t n?i.");

			int[] result = new int[address.Length];
			// Ch?y m?i l?n d?c trong m?t Task riêng bi?t
			var tasks = address.Select((address, index) =>
				Task.Run(() =>
				{
					var value = _modbusClient.ReadHoldingRegisters(address, quantity);
					return value[0]; // L?y giá tr? d?u tiên
					
				})
			).ToArray();

			// Ch? t?t c? các Task hoàn thành
			result = await Task.WhenAll(tasks);

			return result;
		}
		public async Task<int[]> ReadConsecutiveRegistersAsync(int startAddress, int quantity)
		{
			if (_modbusClient == null)
				throw new InvalidOperationException("? ModbusClient chua du?c k?t n?i.");

			return await Task.Run(() => _modbusClient.ReadHoldingRegisters(startAddress, quantity));
		}
		public async Task Disconnect()
		{
			_modbusClient.Disconnect();
		}




	}
}
