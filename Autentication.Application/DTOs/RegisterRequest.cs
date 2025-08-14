using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs
{
    public sealed record RegisterRequest(
        int? IdUsuarioGeneral,        // si ya existe en General.Usuarios (si no, déjalo null)
        string Username,
        string Password,
        IEnumerable<int>? RolesIds    // Ids de Autorizacion.Roles a asignar (opcional)
    );
}
