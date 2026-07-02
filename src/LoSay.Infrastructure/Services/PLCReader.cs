using System.Net.Sockets;
using System.Threading;
using EasyModbus;
using Serilog;
namespace LoSay.Infrastructure.Services;

public class PLCReader
{
	private ModbusClient _modbusClient;
	private readonly int _reconnectDelay = 1000;
	private readonly ILogger _logger;
	//private readonly object _lock = new();
	//private bool _isConnected = false;
	public PLCReader(ModbusClient modbusClient, ILogger logger)
	{
		_modbusClient = modbusClient;
		_logger = logger;
	}

	public async Task<bool> Connect()
	{
		bool isConntected = false;
		try
		{
			await Task.Run(() => _modbusClient.Connect());
			isConntected = true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($" Connect error: {ex.Message}");
		}
		return isConntected;
	}
	//public async Task<bool> Connect()
	//{
	//	lock (_lock)
	//	{
	//		if (_isConnected) return true; // đã connect rồi
	//	}

	//	try
	//	{
	//		await Task.Run(() => _modbusClient.Connect());
	//		lock (_lock)
	//		{
	//			_isConnected = true;
	//		}
	//		return true;
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine($"Connect error: {ex.Message}");
	//		return false;
	//	}
	//}
	//public bool IsConnected => _isConnected;
	public async Task ReConnect()
	{
		try
		{
			await Disconnect();
		}
		catch { }
		await Task.Delay(_reconnectDelay);

		var result = await Connect();
		if (result)
		{
			Console.WriteLine($"ReConnect success.");
		}
		else
		{
			Console.WriteLine($"ReConnect fail.");
		}
	}

	public async Task<int> ReadRegisterAsync(int address)
	{
		if (_modbusClient == null)
			throw new InvalidOperationException("ModbusClient not connected.");

		int[] registers = await Task.Run(() => _modbusClient.ReadHoldingRegisters(address, 1));
		return registers[0];
	}
	/// <summary>
	/// Đọc dữ liệu tại ô nhớ thanh ghi đầu tiên
	/// </summary>
	/// <param name="address"></param>
	/// <param name="quantity"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public async Task<int[]> ReadListRegisterAsync(int[] address, int quantity)
	{
		if (_modbusClient == null)
			throw new InvalidOperationException("ModbusClient chưa được kết nối.");

		int[] result = new int[address.Length];
		// Chạy mỗi lần đọc trong một Task riêng biệt
		var tasks = address.Select((address, index) =>
			Task.Run(() =>
			{
				var value = _modbusClient.ReadHoldingRegisters(address, quantity);
				return value[0]; // Lấy giá trị đầu tiên

			})
		).ToArray();

		// Chờ tất cả các Task hoàn thành
		result = await Task.WhenAll(tasks);

		return result;
	}
	public async Task<int[]> ReadConsecutiveRegistersAsync(int startAddress, int quantity)
	{
		if (_modbusClient == null)
			throw new InvalidOperationException("ModbusClient not connected.");
		try
		{
			if (!_modbusClient.Connected)
			{
				_logger.Error("Lost connection. \nReconnecting...");
				await Connect();
			}
			return await Task.Run(() => _modbusClient.ReadHoldingRegisters(startAddress, quantity));
		}
		catch (IOException ioEx)
		{
			Console.WriteLine($"IO Error: {ioEx.Message}.\nReconnecting...");
			await ReConnect();
			return await RetryRead(startAddress, quantity);
		}
		catch (SocketException sockEx)
		{
			Console.WriteLine($"Socket Error: {sockEx.Message}.\nReconnecting...");
			await ReConnect();
			return await RetryRead(startAddress, quantity);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error read Modbus: {ex.Message}");
			throw;
		}

		//	return await Task.Run(() => _modbusClient.ReadHoldingRegisters(startAddress, quantity));
	}
	private async Task<int[]> RetryRead(int startAddress, int quantity)
	{
		try
		{
			return await Task.Run(() => _modbusClient.ReadHoldingRegisters(startAddress, quantity));
		}
		catch (Exception ex)
		{
			_logger.Error($"Retry fail: {ex.Message} \nReturn temp data");
			return new int[quantity];
		}
	}
	public async Task Disconnect()
	{

		try
		{
			await Task.Run(() => _modbusClient.Disconnect());
			_logger.Information($"Disconnect success.");
		}
		catch (Exception ex)
		{
			_logger.Error($"Disconnect error: {ex.Message}");
		}
	}

}
