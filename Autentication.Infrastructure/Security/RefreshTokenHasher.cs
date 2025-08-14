using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Infrastructure.Security
{
    public static class RefreshTokenHasher
    {
        public static string Hash(string opaque)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(opaque)));
        }

        public static string GenerateOpaque() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
