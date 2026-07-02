using Microsoft.EntityFrameworkCore;

namespace LoSay.Application.Common.Interfaces
{
	public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
	{
		Task<int> CommitAsync();
	}
}
