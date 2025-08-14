using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class Rol : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int IdAplicacion { get; set; }

        public string Nombre { get; set; } = default!;
        public string Descripcion { get; set; } = default!;

        // Navigation
        //public Aplicacion? Aplicacion { get; set; }
    }
}
