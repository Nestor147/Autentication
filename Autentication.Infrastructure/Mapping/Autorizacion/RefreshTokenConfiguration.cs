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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", "Autorizacion");

            // PK (GUID generado por la app)
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .ValueGeneratedNever()
                   .HasColumnName("Id");

            // Columns
            builder.Property(e => e.IdUsuarioSistema)
                   .IsRequired()
                   .HasColumnName("IdUsuarioSistema");

            builder.Property(e => e.TokenHash)
                   .IsRequired()
                   .HasMaxLength(500)
                   .HasColumnName("TokenHash");

            builder.Property(e => e.FechaExpiracion)
                   .IsRequired()
                   .HasColumnType("datetime")
                   .HasColumnName("FechaExpiracion");

            builder.Property(e => e.Usado)
                   .IsRequired()
                   .HasDefaultValue(false)
                   .HasColumnName("Usado");

            builder.Property(e => e.Revocado)
                   .IsRequired()
                   .HasDefaultValue(false)
                   .HasColumnName("Revocado");

            builder.Property(e => e.FechaCreacion)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaCreacion");

            builder.Property(e => e.IP)
                   .HasMaxLength(100)
                   .HasColumnName("IP");

            builder.Property(e => e.UserAgent)
                   .HasMaxLength(500)
                   .HasColumnName("UserAgent");

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

            // FK -> Autorizacion.UsuariosSistema(Id)
            //builder.HasOne(e => e.UsuarioSistema)
            //       .WithMany() // si agregas ICollection<RefreshToken> en UsuarioSistema, cambia a .WithMany(u => u.RefreshTokens)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_RefreshToken_Usuario");

            //// Índices útiles
            //builder.HasIndex(e => e.IdUsuarioSistema)
            //       .HasDatabaseName("IX_RefreshTokens_IdUsuarioSistema");

            //builder.HasIndex(e => new { e.IdUsuarioSistema, e.Revocado, e.Usado })
            //       .HasDatabaseName("IX_RefreshTokens_Usuario_Estados");

            //// (Opcional) Unicidad del hash si así lo requiere tu negocio
            //// builder.HasIndex(e => e.TokenHash).IsUnique().HasDatabaseName("UX_RefreshTokens_TokenHash");
        }
    }
}
