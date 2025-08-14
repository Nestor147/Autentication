using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class AuditoriaLogin : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int? IdUsuarioSistema { get; set; }

        public string? Username { get; set; }     // NVARCHAR(50) NULL
        public string? IP { get; set; }           // NVARCHAR(100) NULL
        public string? UserAgent { get; set; }    // NVARCHAR(500) NULL
        public bool? Exitoso { get; set; }        // BIT NULL
        public string? Mensaje { get; set; }      // NVARCHAR(250) NULL
        public DateTime? Fecha { get; set; }      // DATETIME DEFAULT GETDATE()

        // Navigation
        //public UsuarioSistema? UsuarioSistema { get; set; }
    }
}
