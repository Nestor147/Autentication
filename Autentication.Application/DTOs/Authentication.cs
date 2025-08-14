using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.DTOs
{
    public sealed record LoginRequest(string Username, string Password, string? DeviceFingerprint);
    public sealed record TokenPair(string AccessToken, string RefreshToken);
    public sealed record RefreshRequest(string RefreshToken);
}
