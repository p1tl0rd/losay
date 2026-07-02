using LoSay.Application.Common.Interfaces;
using LoSay.Domain.Entities;

namespace LoSay.Application.Repositories
{
	public interface IPLCDataRepository : IRepositoryBase<PLCData, int>
	{
		Task<int> CreatePLCDataAsync(PLCData pLCData);
		/// <summary>
		/// Lấy dữ liệu PLC đã lưu trong DB theo machineId. Giới hạn dữ liệu theo số giờ truyền vào.
		/// </summary>
		/// <param name="id"machine Id></param>
		/// <param name="timeSpan">Số giờ giới hạn muốn lấy ra theo máy</param>
		/// <returns>PLCData</returns>
		Task<IEnumerable<PLCData>> GetPLCDataByMachineIdAync(int id, TimeSpan timeSpan);

		Task<int> CreatePLCData(PLCData pLCData);

		//Read data to create Chart
		Task<IEnumerable<PLCData>> GetPLCDataByLotNoAndMachineIdAsync(int machineId, string lotNo);
		Task UpdateLotNoAsync(List<PLCData> pLCDatas);
	}
}
