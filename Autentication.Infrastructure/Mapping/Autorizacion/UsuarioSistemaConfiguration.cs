using Autentication.Core.Entities.Autorizacion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Infrastructure.Mapping.Autorizacion
{
    public class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
    {
        public void Configure(EntityTypeBuilder<UsuarioSistema> builder)
        {
            builder.ToTable("UsuariosSistema", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Foreign Key
            builder.Property(e => e.IdUsuarioGeneral)
                   .IsRequired()
                   .HasColumnName("IdUsuarioGeneral");

            // Columns
            builder.Property(e => e.Username)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnName("Username");

            builder.Property(e => e.Password)
                   .IsRequired()
                   .HasMaxLength(300)
                   .HasColumnName("Password");

            builder.Property(e => e.UltimoCambio)
                   .HasColumnType("datetime")
                   .HasColumnName("UltimoCambio");

            builder.Property(e => e.Locked)
                   .IsRequired()
                   .HasDefaultValue(false)
                   .HasColumnName("Locked");

            builder.Property(e => e.LockDate)
                   .HasColumnType("datetime")
                   .HasColumnName("LockDate");

            builder.Property(e => e.NuevoUsuario)
                   .IsRequired()
                   .HasDefaultValue(false)
                   .HasColumnName("NuevoUsuario");

            builder.Property(e => e.LoginPerpetuo)
                   .IsRequired()
                   .HasDefaultValue(true)
                   .HasColumnName("LoginPerpetuo");

            builder.Property(e => e.LoginClave)
                   .IsRequired()
                   .HasDefaultValue(true)
                   .HasColumnName("LoginClave");

            // Audit / Estado
            builder.Property(e => e.EstadoRegistro)
                   .IsRequired()
                   .HasDefaultValue(1)
                   .HasColumnName("EstadoRegistro");

            builder.Property(e => e.UsuarioRegistro)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasDefaultValueSql("SYSTEM_USER")
                   .HasColumnName("UsuarioRegistro");

            builder.Property(e => e.FechaRegistro)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaRegistro");

            //// FK -> General.Usuarios(Id)
            //builder.HasOne(e => e.UsuarioGeneral)
            //       .WithMany() // si agregas ICollection<UsuarioSistema> en Usuario, cambia por .WithMany(u => u.UsuariosSistema)
            //       .HasForeignKey(e => e.IdUsuarioGeneral)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_UsuariosSistema_Usuarios");

            //// (Opcional) Índices sugeridos:
            //// builder.HasIndex(e => e.Username).IsUnique().HasDatabaseName("UX_UsuariosSistema_Username");
            //// Nota: agrega esto solo si también lo creas en la BD para mantener consistencia.
        }
    }
}
