using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs.Atacado
{
    public sealed class CreateBuyerResponse
    {
        public int UserId { get; init; }
        public string Username { get; init; } = default!;
        public string RoleAssigned { get; init; } = "COMPRADOR";
    }
}
