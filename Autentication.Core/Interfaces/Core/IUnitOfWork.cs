using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Interfaces.Core
{
    public interface IUnitOfWork : IDisposable
    {
        // IUnitOfWork.cs
        //public IGenericRepository<Pais> PaisRepository { get; }
      
        IGenericRepository<T> Repository<T>() where T : class;

        int SaveChanges();
        Task<int> SaveChangesAsync();

        // Transacciones sincronas (puedes dejarlas si las necesitas)
        IDbContextTransaction BeginTransaction();
        void Commit();
        void Rollback();

        // ✅ Transacciones asíncronas (necesarias)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
