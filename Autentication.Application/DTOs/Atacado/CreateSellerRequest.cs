using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs.Atacado
{
    public sealed class CreateSellerRequest
    {
        public string Username { get; init; } = default!;
        public string Password { get; init; } = default!;
        public int? IdUsuarioGeneral { get; init; } // opcional si enlazas con otra tabla
    }
}
