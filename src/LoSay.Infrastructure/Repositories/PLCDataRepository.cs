using LoSay.Application.Common.Interfaces;
using LoSay.Application.Repositories;
using LoSay.Data.Contexts;
using LoSay.Domain.Entities;
using LoSay.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Repositories
{
	public class PLCDataRepository : RepositoryBase<PLCData, int>, IPLCDataRepository
	{
		public PLCDataRepository(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork) : base(context, unitOfWork)
		{
		}

		public async Task<int> CreatePLCDataAsync(PLCData pLCData) => await CreateAsync(pLCData);
		public async Task<IEnumerable<PLCData>> GetPLCDataByMachineIdAync(int id, TimeSpan timeSpan)
		{
			var endTime = DateTime.Now;
			var startTime = endTime - timeSpan;
			var plcDatas = FindByCondition(p => p.MachineId.Equals(id) && p.Timestamp >= startTime && p.Timestamp <= endTime).OrderBy(p => p.Timestamp);

			return plcDatas.ToList();

		}

		// Read data to create Chart
		public async Task<IEnumerable<PLCData>> GetPLCDataByLotNoAndMachineIdAsync(int machineId, string lotNo)
		{
			return await FindByCondition(p => p.MachineId.Equals(machineId) && p.LotNo.Equals(lotNo))
				.OrderBy(p => p.Timestamp).ToListAsync();
		}

		public async Task<int> CreatePLCData(PLCData pLCData) => await CreateAsync(pLCData);

		public async Task UpdateLotNoAsync(List<PLCData> pLCDatas)
		=> await UpdateListAsync(pLCDatas);

	}
}
