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
    public class AuditoriaLoginConfiguration : IEntityTypeConfiguration<AuditoriaLogin>
    {
        public void Configure(EntityTypeBuilder<AuditoriaLogin> builder)
        {
            builder.ToTable("AuditoriaLogins", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdUsuarioSistema)
                   .HasColumnName("IdUsuarioSistema");

            builder.Property(e => e.Username)
                   .HasMaxLength(50)
                   .HasColumnName("Username");

            builder.Property(e => e.IP)
                   .HasMaxLength(100)
                   .HasColumnName("IP");

            builder.Property(e => e.UserAgent)
                   .HasMaxLength(500)
                   .HasColumnName("UserAgent");

            builder.Property(e => e.Exitoso)
                   .HasColumnName("Exitoso");

            builder.Property(e => e.Mensaje)
                   .HasMaxLength(250)
                   .HasColumnName("Mensaje");

            builder.Property(e => e.Fecha)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("Fecha");

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
            //       .WithMany() // si agregas ICollection<AuditoriaLogin> en UsuarioSistema, cambia a .WithMany(u => u.AuditoriasLogin)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_Auditoria_Usuario");

            //// Índices útiles (opcional)
            //builder.HasIndex(e => e.IdUsuarioSistema).HasDatabaseName("IX_AuditoriaLogins_IdUsuarioSistema");
            //builder.HasIndex(e => e.Fecha).HasDatabaseName("IX_AuditoriaLogins_Fecha");
            //builder.HasIndex(e => e.Username).HasDatabaseName("IX_AuditoriaLogins_Username");
        }
    }
}
