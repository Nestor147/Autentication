using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class RefreshToken : AuditFieldsQueryFilter
    {
        public Guid Id { get; set; }                    // UNIQUEIDENTIFIER (ValueGeneratedNever)
        public int IdUsuarioSistema { get; set; }

        public string TokenHash { get; set; } = default!;   // NVARCHAR(500)
        public DateTime FechaExpiracion { get; set; }       // DATETIME
        public bool Usado { get; set; }                     // BIT
        public bool Revocado { get; set; }                  // BIT
        public DateTime? FechaCreacion { get; set; }        // DATETIME (default GETDATE())
        public string? IP { get; set; }                     // NVARCHAR(100)
        public string? UserAgent { get; set; }              // NVARCHAR(500)

        // Navigation
        //public UsuarioSistema? UsuarioSistema { get; set; }
    }
}
