using System.Linq.Expressions;
using LoSay.Application.Common.Interfaces;
using LoSay.Data.Contexts;
using LoSay.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LoSay.Infrastructure.Common
{
	public class RepositoryQueryBase<T, K> : IRepositoryQueryBase<T, K> where T : EntityBase<K>
	{
		private readonly ApplicationDbContext _context;
		public RepositoryQueryBase(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
		public IQueryable<T> FindAll(bool trackChanges = false) => !trackChanges ? _context.Set<T>().AsNoTracking() : _context.Set<T>();

		public IQueryable<T> FindAll(bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
		{
			var items = FindAll(trackChanges);
			items = includeProperties.Aggregate(items, (current, includeProperties) => current.Include(includeProperties));
			return items;
		}

		public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false)
			=> !trackChanges ? _context.Set<T>().Where(expression).AsNoTracking() : _context.Set<T>().Where(expression);

		public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = false, params Expression<Func<T, object>>[] includeProperties)
		{
			var items = FindByCondition(expression, trackChanges);
			items = includeProperties.Aggregate(items, (current, includeProperties) => current.Include(includeProperties));
			return items;
		}

		public async Task<T?> GetByIdAsync(K id) => await FindByCondition(x => x.Id.Equals(id)).FirstOrDefaultAsync();

		public async Task<T?> GetByIdAsync(K id, params Expression<Func<T, object>>[] includeProperties)
			=> await FindByCondition(x => x.Equals(id), trackChanges: false, includeProperties: includeProperties).FirstOrDefaultAsync();
	}
}
