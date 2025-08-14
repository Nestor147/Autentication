using Autentication.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Autorizacion
{
    public class TokenRevocado : AuditFieldsQueryFilter
    {
        public Guid Jti { get; set; }                 // UNIQUEIDENTIFIER (ValueGeneratedNever)
        public int IdUsuarioSistema { get; set; }

        public string? Motivo { get; set; }           // NVARCHAR(250) NULL
        public DateTime? FechaRevocacion { get; set; }// DATETIME DEFAULT GETDATE()

        // Navigation
        //public UsuarioSistema? UsuarioSistema { get; set; }
    }
}
