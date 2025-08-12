using Autentication.Core.Entities.Core;
using Autentication.Core.Interfaces.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Infrastructure.Context.Core
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public T GetById(int id) => _dbSet.Find(id);
        public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public T GetByCustom(Func<IQueryable<T>, T> query)
            => query(_dbSet.AsQueryable());

        public async Task<IEnumerable<T>> GetByCustomQuery(Func<IQueryable<T>, IEnumerable<T>> query)
            => query(_dbSet.AsQueryable());

        public IEnumerable<T> GetAll() => _dbSet.ToList();
        public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate)
            => _dbSet.Where(predicate).ToList();

        public ResponsePostDetail Insert(T entity, string proceso = "INSERT")
        {
            _dbSet.Add(entity);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail Insert(List<T> entities, string proceso = "INSERT")
        {
            _dbSet.AddRange(entities);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public async Task<ResponsePostDetail> InsertAsync(T entity, string proceso = "INSERT")
        {
            await _dbSet.AddAsync(entity);
            int affected = await _context.SaveChangesAsync();
            string idGenerado = null;
            var idProp = entity.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.StartsWith("Id") &&
                                     (p.PropertyType == typeof(int) || p.PropertyType == typeof(string)));
            if (idProp != null)
            {
                var idValue = idProp.GetValue(entity);
                idGenerado = idValue?.ToString();
            }
            return new ResponsePostDetail
            {
                Proceso = proceso,
                FilasAfectadas = affected,
                IdGenerado = idGenerado
            };
        }


        public async Task<ResponsePostDetail> InsertAsync(List<T> entities, string proceso = "INSERT")
        {
            await _dbSet.AddRangeAsync(entities);
            int affected = await _context.SaveChangesAsync();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail Update(T entity, string proceso = "UPDATE")
        {
            _dbSet.Update(entity);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail Update(List<T> entities, string proceso = "UPDATE")
        {
            _dbSet.UpdateRange(entities);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail UpdateCustom(T entity, params Expression<Func<T, object>>[] includeProperties)
        {
            var entry = _context.Entry(entity);
            foreach (var prop in includeProperties)
            {
                entry.Property(prop).IsModified = true;
            }

            int affected = _context.SaveChanges();
            return new ResponsePostDetail
            {
                Proceso = "UPDATE CUSTOM",
                FilasAfectadas = affected
            };
        }

        public async Task<ResponsePostDetail> DeleteAsync(int id, string proceso = "DELETE")
        {
            var entity = await GetByIdAsync(id);
            _dbSet.Remove(entity);
            int affected = await _context.SaveChangesAsync();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail Delete(T entity, string proceso = "DELETE")
        {
            _dbSet.Remove(entity);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        public ResponsePostDetail Delete(List<T> entities, string proceso = "DELETE")
        {
            _dbSet.RemoveRange(entities);
            int affected = _context.SaveChanges();
            return new ResponsePostDetail { Proceso = proceso, FilasAfectadas = affected };
        }

        //transacciones controladas para afectar a multiples tablas 
        public Task AddAsyncWithoutSave(T entity)
        {
            return _dbSet.AddAsync(entity).AsTask();
        }
        public Task AddRangeAsyncWithoutSave(List<T> entities)
        {
            _dbSet.AddRange(entities);
            return Task.CompletedTask;
        }
        public void UpdateWithoutSave(T entity)
        {
            _dbSet.Update(entity);
        }
        public void UpdateRangeWithoutSave(List<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }
        public void RemoveWithoutSave(T entity)
        {
            _dbSet.Remove(entity);
        }
        public void RemoveRangeWithoutSave(List<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

    }
}
