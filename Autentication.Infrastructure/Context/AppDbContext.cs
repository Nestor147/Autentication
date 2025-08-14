using Autentication.Core.Entities.Autorizacion;
using Autentication.Infrastructure.Mapping.Autorizacion;
using Microsoft.EntityFrameworkCore;

namespace Autentication.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<Aplicacion> Aplicaciones { get; set; }
        public virtual DbSet<AuditoriaLogin> AuditoriaLogin { get; set; }
        public virtual DbSet<DispositivoConocido> DispositivoConocido { get; set; }
        public virtual DbSet<IntentoFallidoLogin> IntentoFallidoLogin { get; set; }
        public virtual DbSet<RefreshToken> RefreshToken { get; set; }
        public virtual DbSet<Rol> Rol { get; set; }
        public virtual DbSet<RolUsuario> RolUsuario { get; set; }
        public virtual DbSet<TokenRevocado> TokenRevocado { get; set; }
        public virtual DbSet<UsuarioSistema> UsuarioSistema { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AplicacionConfiguration());
            modelBuilder.ApplyConfiguration(new AuditoriaLoginConfiguration());
            modelBuilder.ApplyConfiguration(new DispositivoConocidoConfiguration());
            modelBuilder.ApplyConfiguration(new IntentoFallidoLoginConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new RolConfiguration());
            modelBuilder.ApplyConfiguration(new RolUsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new TokenRevocadoConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioSistemaConfiguration());
            base.OnModelCreating(modelBuilder);
            // Aquí mapeos personalizados
        }
    }
}
