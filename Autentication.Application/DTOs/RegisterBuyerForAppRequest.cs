using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs
{
    public sealed record RegisterBuyerRequest(
        string Username,
        string Password
    );
}
