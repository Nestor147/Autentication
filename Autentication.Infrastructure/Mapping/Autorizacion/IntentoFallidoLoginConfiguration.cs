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
    public class IntentoFallidoLoginConfiguration : IEntityTypeConfiguration<IntentoFallidoLogin>
    {
        public void Configure(EntityTypeBuilder<IntentoFallidoLogin> builder)
        {
            builder.ToTable("IntentosFallidosLogin", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdUsuarioSistema)
                   .HasColumnName("IdUsuarioSistema");

            builder.Property(e => e.Username)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnName("Username");

            builder.Property(e => e.IP)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("IP");

            builder.Property(e => e.UserAgent)
                   .HasMaxLength(500)
                   .HasColumnName("UserAgent");

            builder.Property(e => e.Exitoso)
                   .IsRequired()
                   .HasDefaultValue(false)
                   .HasColumnName("Exitoso");

            builder.Property(e => e.FechaIntento)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaIntento");

            // Audit / Estado
            builder.Property(e => e.EstadoRegistro)
                   .IsRequired()
                   .HasDefaultValue(1)
                   .HasColumnName("EstadoRegistro");

            builder.Property(e => e.FechaRegistro)
                   .IsRequired()
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaRegistro");

            builder.Property(e => e.UsuarioRegistro)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasDefaultValueSql("SYSTEM_USER")
                   .HasColumnName("UsuarioRegistro");

            //// FK -> Autorizacion.UsuariosSistema(Id) (nullable)
            //builder.HasOne(e => e.UsuarioSistema)
            //       .WithMany() // si agregas ICollection<IntentoFallidoLogin> en UsuarioSistema, cambia a .WithMany(u => u.IntentosFallidos)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_Intento_Usuario");

            //// Índices sugeridos (útiles para auditoría/consultas)
            //builder.HasIndex(e => e.IdUsuarioSistema).HasDatabaseName("IX_Intentos_IdUsuarioSistema");
            //builder.HasIndex(e => e.Username).HasDatabaseName("IX_Intentos_Username");
            //builder.HasIndex(e => e.FechaIntento).HasDatabaseName("IX_Intentos_FechaIntento");
        }
    }
}
