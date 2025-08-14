using Autentication.Core.Entities.Autorizacion;
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
        public IGenericRepository<Aplicacion> AplicacionRepository { get; }
        public IGenericRepository<AuditoriaLogin> AuditoriaLoginRepository { get; }
        public IGenericRepository<DispositivoConocido> DispositivoConocidoRepository { get; }
        public IGenericRepository<IntentoFallidoLogin> IntentoFallidoLoginRepository { get; }
        public IGenericRepository<RefreshToken> RefreshTokenRepository { get; }
        public IGenericRepository<Rol> RolRepository { get; }
        public IGenericRepository<RolUsuario> RolUsuarioRepository { get; }
        public IGenericRepository<TokenRevocado> TokenRevocadoRepository { get; }
        public IGenericRepository<UsuarioSistema> UsuarioSistemaRepository { get; }

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
