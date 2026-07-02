using LoSay.Application.Common.Interfaces;
using LoSay.Data.Contexts;
using LoSay.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoSay.Infrastructure.Common
{
	public class RepositoryBase<T, K> : RepositoryQueryBase<T, K>, IRepositoryBase<T, K> where T : EntityBase<K>
	{
		private readonly ApplicationDbContext _context;
		private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
		public RepositoryBase(ApplicationDbContext context, IUnitOfWork<ApplicationDbContext> unitOfWork) : base(context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();

		public async Task<K> CreateAsync(T entity)
		{
			await _context.Set<T>().AddAsync(entity);
			return entity.Id;
		}

		public async Task<IList<K>> CreateListAsync(IEnumerable<T> entities)
		{
			await _context.Set<T>().AddRangeAsync(entities);
			return entities.Select(e => e.Id).ToList();
		}
		public Task UpdateAsync(T entity)
		{
			if (_context.Entry(entity).State == EntityState.Unchanged) return Task.CompletedTask;
			T exist = _context.Set<T>().Find(entity.Id);
			_context.Entry(exist).CurrentValues.SetValues(entity);

			return Task.CompletedTask;
		}

		public Task UpdateListAsync(IEnumerable<T> entities)
		{
			_context.ChangeTracker.Clear();
			_context.Set<T>().UpdateRange(entities);

			return Task.CompletedTask;
		}

		public Task DeleteAsync(T entity)
		{
			_context.Set<T>().Remove(entity);
			return Task.CompletedTask;
		}

		public Task DeleteListAsync(IEnumerable<T> entities)
		{
			_context.Set<T>().RemoveRange(entities);

			return Task.CompletedTask;
		}

		public async Task EndTransactionAsync()
		{
			await _context.Database.CommitTransactionAsync();
		}

		public async Task RollBackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

		public Task<int> SaveChangeAsync() => _unitOfWork.CommitAsync();


	}
}
