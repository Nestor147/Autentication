using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class RolUsuario : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int IdRol { get; set; }
        public int IdUsuarioSistema { get; set; }

        //// Navigation
        //public Rol? Rol { get; set; }
        //public UsuarioSistema? UsuarioSistema { get; set; }
    }
}
