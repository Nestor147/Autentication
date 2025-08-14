using Autentication.Core.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Interfaces.Core
{
    public interface IGenericRepository<T> where T : class
    {
        // Búsquedas
        T GetById(int id);
        Task<T> GetByIdAsync(int id);
        T GetByCustom(Func<IQueryable<T>, T> query);
        IEnumerable<T> GetAll();
        Task<List<T>> GetAllAsync();
        Task<IEnumerable<T>> GetByCustomQuery(Func<IQueryable<T>, IEnumerable<T>> query);
        Task<IEnumerable<T>> Find(Expression<Func<T, bool>> predicate);

        // Inserción
        ResponsePostDetail Insert(T entity, string proceso = "INSERT");
        ResponsePostDetail Insert(List<T> entities, string proceso = "INSERT");
        Task<ResponsePostDetail> InsertAsync(T entity, string proceso = "INSERT");
        Task<ResponsePostDetail> InsertAsync(List<T> entities, string proceso = "INSERT");

        // Actualización
        ResponsePostDetail Update(T entity, string proceso = "UPDATE");
        ResponsePostDetail Update(List<T> entities, string proceso = "UPDATE");
        ResponsePostDetail UpdateCustom(T entity, params Expression<Func<T, object>>[] includeProperties);

        // Eliminación
        ResponsePostDetail Delete(T entity, string proceso = "DELETE");
        ResponsePostDetail Delete(List<T> entities, string proceso = "DELETE");
        Task<ResponsePostDetail> DeleteAsync(int id, string proceso = "DELETE");


        IQueryable<T> Query(bool asNoTracking = true);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);


    }
}
