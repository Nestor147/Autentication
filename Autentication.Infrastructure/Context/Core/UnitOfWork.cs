using Autentication.Core.Interfaces.Core;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Infrastructure.Context.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Atributos
        private readonly AppDbContext _context;
        private IDbContextTransaction _transaction;
        //private readonly IGenericRepository<Pais> _paisRepository;


        // Repositorios genéricos por tipo
        private readonly ConcurrentDictionary<Type, object> _repositories = new();


        #endregion

        #region Constructor
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        #endregion


        #region Metodos
        //public IGenericRepository<Pais> PaisRepository => _paisRepository ?? new GenericRepository<Pais>(_context);

        #endregion

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.TryGetValue(typeof(T), out var repo))
                return (IGenericRepository<T>)repo;

            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.TryAdd(typeof(T), repositoryInstance);
            return repositoryInstance;
        }

        public int SaveChanges() => _context.SaveChanges();
        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

        public IDbContextTransaction BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
            return _transaction;
        }

        public void Commit() => _transaction?.Commit();
        public void Rollback() => _transaction?.Rollback();

        public void Dispose()
        {
            _transaction?.Dispose();
            //_context.Dispose();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
                _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

    }
}
