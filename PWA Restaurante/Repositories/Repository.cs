using PWA_Restaurante.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace PWA_Restaurante.Repositories
{
	public class Repository<T> where T : class
	{
		private readonly RestauranteContext _context;
		
		public Repository(RestauranteContext context)
		{
			_context = context;
		}
		public IEnumerable<T> GetAll()
		{
			return _context.Set<T>().AsNoTracking();
		}

		public T? Get(object id)
		{
			return _context.Find<T>(id);
		}
		
		public void Insert(T entity)
		{
			_context.Add(entity);
			_context.SaveChanges();
		}
		
		public void Update(T entity)
		{
			_context.Update(entity);
			_context.SaveChanges();
		}
		
		public void Delete(object id)
		{
			var entity = _context.Find<T>(id);
			if (entity != null)
			{
				_context.Remove(entity);
				_context.SaveChanges();
			}
		}

		public void Delete(T entity)
		{
			_context.Remove(entity);
			_context.SaveChanges();
		}

		public IEnumerable<T> GetAllWithTracking()
		{
			return _context.Set<T>();
		}

		public IQueryable<T> GetQueryable()
		{
			return _context.Set<T>();
		}

		public T? GetById(object id)
		{
			return _context.Set<T>().AsNoTracking().FirstOrDefault(e => EF.Property<object>(e, "Id").Equals(id));
		}

		public T? GetByIdWithTracking(object id)
		{
			return _context.Set<T>().FirstOrDefault(e => EF.Property<object>(e, "Id").Equals(id));
		}

		//tracking/no tracking para metodos de lectura o si hará cambios
	}
}
