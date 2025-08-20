using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Password
{
    public interface IPasswordHasher
    {
        string Hash(string plaintext);
        bool Verify(string hash, string plaintext);
    }
}
