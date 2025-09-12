using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Services.Exceptions
{
    // Common/DomainExceptions.cs
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }
        protected DomainException(string code, string message) : base(message) => ErrorCode = code;
    }

    // Auth / Registro
    public sealed class UserDuplicateException : DomainException { public UserDuplicateException(string u) : base("USER_DUPLICATE", $"El username '{u}' ya existe.") { } }
    public sealed class WeakPasswordException : DomainException { public WeakPasswordException(string m) : base("WEAK_PASSWORD", m) { } }
    public sealed class AppNotFoundException : DomainException { public AppNotFoundException(string a) : base("APP_NOT_FOUND", $"La aplicación '{a}' no existe o está inactiva.") { } }
    public sealed class RoleNotFoundException : DomainException { public RoleNotFoundException(string r) : base("ROLE_NOT_FOUND", $"El rol '{r}' no existe para la aplicación indicada.") { } }

    // Login / Tokens
    public sealed class UserNotFoundException : DomainException { public UserNotFoundException() : base("USER_NOT_FOUND", "Usuario no encontrado.") { } }
    public sealed class UserLockedException : DomainException { public UserLockedException() : base("USER_LOCKED", "Usuario bloqueado.") { } }
    public sealed class InvalidCredentialsException : DomainException { public InvalidCredentialsException() : base("INVALID_CREDENTIALS", "Credenciales inválidas.") { } }
    public sealed class RefreshInvalidException : DomainException { public RefreshInvalidException() : base("REFRESH_INVALID", "Refresh inválido o expirado.") { } }

}
