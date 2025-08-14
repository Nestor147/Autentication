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
    public class RolUsuarioConfiguration : IEntityTypeConfiguration<RolUsuario>
    {
        public void Configure(EntityTypeBuilder<RolUsuario> builder)
        {
            builder.ToTable("RolesUsuarios", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdRol)
                   .IsRequired()
                   .HasColumnName("IdRol");

            builder.Property(e => e.IdUsuarioSistema)
                   .IsRequired()
                   .HasColumnName("IdUsuarioSistema");

            // Audit / Estado
            builder.Property(e => e.EstadoRegistro)
                   .IsRequired()
                   .HasDefaultValue(1)
                   .HasColumnName("EstadoRegistro");

            builder.Property(e => e.FechaRegistro)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaRegistro");

            builder.Property(e => e.UsuarioRegistro)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasDefaultValueSql("SYSTEM_USER")
                   .HasColumnName("UsuarioRegistro");

            //// FK -> Autorizacion.Roles(Id)
            //builder.HasOne(e => e.Rol)
            //       .WithMany() // si agregas ICollection<RolUsuario> en Rol, usa .WithMany(r => r.RolesUsuarios)
            //       .HasForeignKey(e => e.IdRol)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_RolesUsuarios_Rol");

            //// FK -> Autorizacion.UsuariosSistema(Id)
            //builder.HasOne(e => e.UsuarioSistema)
            //       .WithMany() // si agregas ICollection<RolUsuario> en UsuarioSistema, usa .WithMany(u => u.RolesUsuarios)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_RolesUsuarios_Usuario");

            //// Índices útiles
            //builder.HasIndex(e => e.IdRol)
            //       .HasDatabaseName("IX_RolesUsuarios_IdRol");

            //builder.HasIndex(e => e.IdUsuarioSistema)
            //       .HasDatabaseName("IX_RolesUsuarios_IdUsuarioSistema");

            //// (Opcional) Unicidad del par Rol-Usuario si tu negocio lo requiere:
            //// builder.HasIndex(e => new { e.IdRol, e.IdUsuarioSistema })
            ////        .IsUnique()
            ////        .HasDatabaseName("UX_RolesUsuarios_Rol_Usuario");
        }
    }
}
