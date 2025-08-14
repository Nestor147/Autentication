using Autentication.Core.Entities.Autorizacion;
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
        private readonly IGenericRepository<Aplicacion> _aplicacionRepository;
        private readonly IGenericRepository<AuditoriaLogin> _auditoriaLoginRepository;
        private readonly IGenericRepository<DispositivoConocido> _dispositivoConocidoRepository;
        private readonly IGenericRepository<IntentoFallidoLogin> _intentoFallidoLoginRepository;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
        private readonly IGenericRepository<Rol> _rolRepository;
        private readonly IGenericRepository<RolUsuario> _rolUsuarioRepository;
        private readonly IGenericRepository<TokenRevocado> _tokenRevocadoRepository;
        private readonly IGenericRepository<UsuarioSistema> _usuarioSistemaRepository;


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
        public IGenericRepository<Aplicacion> AplicacionRepository => _aplicacionRepository ?? new GenericRepository<Aplicacion>(_context);

        public IGenericRepository<AuditoriaLogin> AuditoriaLoginRepository => _auditoriaLoginRepository ?? new GenericRepository<AuditoriaLogin>(_context);

        public IGenericRepository<DispositivoConocido> DispositivoConocidoRepository => _dispositivoConocidoRepository ?? new GenericRepository<DispositivoConocido>(_context);

        public IGenericRepository<IntentoFallidoLogin> IntentoFallidoLoginRepository => _intentoFallidoLoginRepository ?? new GenericRepository<IntentoFallidoLogin>(_context);

        public IGenericRepository<RefreshToken> RefreshTokenRepository => _refreshTokenRepository ?? new GenericRepository<RefreshToken>(_context);

        public IGenericRepository<Rol> RolRepository => _rolRepository ?? new GenericRepository<Rol>(_context);

        public IGenericRepository<RolUsuario> RolUsuarioRepository => _rolUsuarioRepository ?? new GenericRepository<RolUsuario>(_context);

        public IGenericRepository<TokenRevocado> TokenRevocadoRepository => _tokenRevocadoRepository ?? new GenericRepository<TokenRevocado>(_context);

        public IGenericRepository<UsuarioSistema> UsuarioSistemaRepository => _usuarioSistemaRepository ?? new GenericRepository<UsuarioSistema>(_context);

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
