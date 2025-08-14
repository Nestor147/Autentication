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
    public class DispositivoConocidoConfiguration : IEntityTypeConfiguration<DispositivoConocido>
    {
        public void Configure(EntityTypeBuilder<DispositivoConocido> builder)
        {
            builder.ToTable("DispositivosConocidos", "Autorizacion");

            // PK
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .ValueGeneratedOnAdd()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdUsuarioSistema)
                   .IsRequired()
                   .HasColumnName("IdUsuarioSistema");

            builder.Property(e => e.FingerprintHash)
                   .IsRequired()
                   .HasMaxLength(300)
                   .HasColumnName("FingerprintHash");

            builder.Property(e => e.NombreDispositivo)
                   .HasMaxLength(100)
                   .HasColumnName("NombreDispositivo");

            builder.Property(e => e.UserAgent)
                   .HasMaxLength(500)
                   .HasColumnName("UserAgent");

            builder.Property(e => e.IP)
                   .HasMaxLength(100)
                   .HasColumnName("IP");

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

            //// FK -> Autorizacion.UsuariosSistema(Id)
            //builder.HasOne(e => e.UsuarioSistema)
            //       .WithMany() // si agregas ICollection<DispositivoConocido> en UsuarioSistema, usa .WithMany(u => u.DispositivosConocidos)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_Dispositivo_Usuario");

            //// Índices útiles
            //builder.HasIndex(e => e.IdUsuarioSistema)
            //       .HasDatabaseName("IX_DispositivosConocidos_IdUsuarioSistema");

            //// (Opcional) Evitar duplicados por mismo usuario y fingerprint:
            //// builder.HasIndex(e => new { e.IdUsuarioSistema, e.FingerprintHash })
            ////        .IsUnique()
            ////        .HasDatabaseName("UX_DispositivosConocidos_Usuario_Fingerprint");
        }
    }
}
