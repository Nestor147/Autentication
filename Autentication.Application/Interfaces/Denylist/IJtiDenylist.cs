using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Interfaces.Denylist
{
    public interface IJtiDenylist
    {
        Task<bool> IsRevokedAsync(string jti);
        // En Auth agregarás también: Task RevokeAsync(string jti, TimeSpan ttl);
    }
}
