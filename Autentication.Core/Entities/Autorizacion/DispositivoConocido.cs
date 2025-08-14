using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class DispositivoConocido : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int IdUsuarioSistema { get; set; }

        public string FingerprintHash { get; set; } = default!; // NVARCHAR(300)
        public string? NombreDispositivo { get; set; }          // NVARCHAR(100) NULL
        public string? UserAgent { get; set; }                  // NVARCHAR(500) NULL
        public string? IP { get; set; }                         // NVARCHAR(100) NULL

        // Navigation
        //public UsuarioSistema? UsuarioSistema { get; set; }
    }
}
