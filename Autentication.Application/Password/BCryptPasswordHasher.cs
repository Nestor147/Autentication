

namespace Autentication.Application.Password
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        private readonly int _workFactor;
        public BCryptPasswordHasher(int workFactor = 12) => _workFactor = workFactor;

        public string Hash(string plaintext)
            => BCrypt.Net.BCrypt.HashPassword(plaintext, workFactor: _workFactor);

        public bool Verify(string hash, string plaintext)
            => BCrypt.Net.BCrypt.Verify(plaintext, hash);
    }
}
