using Autentication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Interfaces
{
    public interface IAuthService
    {
        Task<TokenPair> LoginAsync(LoginRequest req, CancellationToken ct);
        Task<TokenPair> RefreshAsync(RefreshRequest req, CancellationToken ct);
        Task LogoutAsync(RefreshRequest req, CancellationToken ct);
        Task<RegisterResponse> RegisterAsync(RegisterRequest req, CancellationToken ct);
        Task ChangePasswordAsync(ChangePasswordRequest req, CancellationToken ct);

        string GetJwks(); // público
    }
}
