using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class IntentoFallidoLogin : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int? IdUsuarioSistema { get; set; }

        public string Username { get; set; } = default!;
        public string IP { get; set; } = default!;
        public string? UserAgent { get; set; }

        public bool Exitoso { get; set; }
        public DateTime? FechaIntento { get; set; }

        // Navigation
    }
}
