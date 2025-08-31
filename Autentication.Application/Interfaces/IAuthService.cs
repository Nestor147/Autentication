using Autentication.Application.DTOs;
using Autentication.Application.DTOs.Atacado;
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
        Task<TokenPair> RegisterBuyerAtacadoAsync(RegisterBuyerRequest req, CancellationToken ct);
        Task ChangePasswordAsync(ChangePasswordRequest req, CancellationToken ct);
        Task<CreateSellerResponse> CreateSellerAsync(CreateSellerRequest req, CancellationToken ct = default);
        Task<CreateBuyerResponse> CreateBuyerAsync(CreateBuyerRequest req, CancellationToken ct = default);
        Task<CreateAdminResponse> CreateAdminAsync(CreateAdminRequest req, CancellationToken ct = default);

        string GetJwks(); // público
    }
}
