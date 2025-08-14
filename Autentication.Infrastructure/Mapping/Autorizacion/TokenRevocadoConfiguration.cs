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
    public class TokenRevocadoConfiguration : IEntityTypeConfiguration<TokenRevocado>
    {
        public void Configure(EntityTypeBuilder<TokenRevocado> builder)
        {
            builder.ToTable("TokensRevocados", "Autorizacion");

            // PK (GUID provisto por la app)
            builder.HasKey(e => e.Jti);
            builder.Property(e => e.Jti)
                   .ValueGeneratedNever()
                   .HasColumnName("Jti");

            // Columns
            builder.Property(e => e.IdUsuarioSistema)
                   .IsRequired()
                   .HasColumnName("IdUsuarioSistema");

            builder.Property(e => e.Motivo)
                   .HasMaxLength(250)
                   .HasColumnName("Motivo");

            builder.Property(e => e.FechaRevocacion)
                   .HasColumnType("datetime")
                   .HasDefaultValueSql("GETDATE()")
                   .HasColumnName("FechaRevocacion");

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
            //       .WithMany() // si agregas ICollection<TokenRevocado> en UsuarioSistema, cambia a .WithMany(u => u.TokensRevocados)
            //       .HasForeignKey(e => e.IdUsuarioSistema)
            //       .OnDelete(DeleteBehavior.NoAction)
            //       .HasConstraintName("FK_TokenRevocado_Usuario");

            //// Índices útiles
            //builder.HasIndex(e => e.IdUsuarioSistema)
            //       .HasDatabaseName("IX_TokensRevocados_IdUsuarioSistema");
        }
    }
}
