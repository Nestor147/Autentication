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
    public class AplicacionConfiguration : IEntityTypeConfiguration<Aplicacion>
    {
        public void Configure(EntityTypeBuilder<Aplicacion> builder)
        {
            builder.ToTable("Aplicaciones", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Required
            builder.Property(e => e.Sigla)
                   .IsRequired()
                   .HasMaxLength(25)
                   .HasColumnName("Sigla");

            builder.Property(e => e.Descripcion)
                   .IsRequired()
                   .HasMaxLength(250)
                   .HasColumnName("Descripcion");

            // Optional
            builder.Property(e => e.Enlace)
                   .HasMaxLength(250)
                   .HasColumnName("Enlace");

            builder.Property(e => e.Icono)
                   .HasMaxLength(50)
                   .HasColumnName("Icono");

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
        }
    }
}
