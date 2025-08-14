using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class Aplicacion : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public string Sigla { get; set; } = default!;
        public string Descripcion { get; set; } = default!;
        public string? Enlace { get; set; }
        public string? Icono { get; set; }
        public int EstadoRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string UsuarioRegistro { get; set; } = default!;
    }
}
