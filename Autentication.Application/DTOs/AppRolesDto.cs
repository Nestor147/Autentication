using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs
{
    public sealed record AppRolesDto(
        int IdAplicacion,
        string Sigla,
        string Descripcion,
        List<string> Roles
    );

}
