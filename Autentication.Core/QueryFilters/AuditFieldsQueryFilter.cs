using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.QueryFilters
{
    public class AuditFieldsQueryFilter
    {
        public int EstadoRegistro { get; set; } = 1;
        public string? UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
