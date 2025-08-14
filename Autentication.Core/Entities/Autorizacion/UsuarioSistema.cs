using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class UsuarioSistema : AuditFieldsQueryFilter
    {
        public int Id { get; set; }
        public int IdUsuarioGeneral { get; set; }

        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;

        public DateTime? UltimoCambio { get; set; }
        public bool Locked { get; set; }
        public DateTime? LockDate { get; set; }

        public bool NuevoUsuario { get; set; }
        public bool LoginPerpetuo { get; set; }
        public bool LoginClave { get; set; }

        // Navigation
        //public Usuario? UsuarioGeneral { get; set; }
    }
}
