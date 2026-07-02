using LoSay.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Common
{
	public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
	{
		private readonly TContext _context;
		public UnitOfWork(TContext context)
		{
			_context = context;
		}
		public Task<int> CommitAsync() => _context.SaveChangesAsync();

		public void Dispose() => _context.Dispose();
	}
}
