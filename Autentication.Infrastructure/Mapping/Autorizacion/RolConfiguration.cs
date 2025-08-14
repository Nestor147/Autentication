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
    public class RolConfiguration : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("Roles", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdAplicacion)
                   .IsRequired()
                   .HasColumnName("IdAplicacion");

            builder.Property(e => e.Nombre)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnName("Nombre");

            builder.Property(e => e.Descripcion)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("Descripcion");

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

            //// FK -> Autorizacion.Aplicaciones(Id)
            //builder.HasOne(e => e.Aplicacion)
            //       .WithMany() // si luego agregas ICollection<Rol> en Aplicacion, cámbialo por .WithMany(a => a.Roles)
            //       .HasForeignKey(e => e.IdAplicacion)
            //       .OnDelete(DeleteBehavior.NoAction) // se alinea con NO ACTION del SQL
            //       .HasConstraintName("FK_Roles_Aplicaciones");

            //// Índice sugerido (opcional, útil para búsquedas por app)
            //builder.HasIndex(e => e.IdAplicacion)
            //       .HasDatabaseName("IX_Roles_IdAplicacion");
        }
    }
}
