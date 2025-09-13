// Autentication.Application.Services.Exceptions/Exceptions.cs
using System;

namespace Autentication.Application.Services.Exceptions
{
    // Base de dominio
    public abstract class DomainException : Exception
    {
        public string ErrorCode { get; }
        protected DomainException(string code, string message) : base(message) => ErrorCode = code;
    }

    // --- Registro / App / Roles ---
    public sealed class UserDuplicateException : DomainException
    {
        public UserDuplicateException(string username)
            : base("USER_DUPLICATE", $"El username '{username}' ya existe.") { }
    }

    public sealed class WeakPasswordException : DomainException
    {
        public WeakPasswordException(string message)
            : base("WEAK_PASSWORD", message) { }
    }

    public sealed class AppNotFoundException : DomainException
    {
        public AppNotFoundException(string app)
            : base("APP_NOT_FOUND", $"La aplicación '{app}' no existe o está inactiva.") { }
    }

    public sealed class RoleNotFoundException : DomainException
    {
        public RoleNotFoundException(string role)
            : base("ROLE_NOT_FOUND", $"El rol '{role}' no existe para la aplicación indicada.") { }
    }

    public sealed class RoleAlreadyAssignedException : DomainException
    {
        public int UserId { get; }
        public int RoleId { get; }
        public RoleAlreadyAssignedException(int userId, int roleId)
            : base("ROLE_ALREADY_ASSIGNED", $"El rol {roleId} ya está asignado al usuario {userId}.")
        {  }
    }

    public sealed class GeneralUserAlreadyLinkedException : DomainException
    {
        public GeneralUserAlreadyLinkedException(int idUsuarioGeneral)
            : base("GENERAL_USER_ALREADY_LINKED", $"El usuario general {idUsuarioGeneral} ya está vinculado.") { }
    }

    // --- Login / Tokens ---
    public sealed class UserNotFoundException : DomainException
    {
        public UserNotFoundException() : base("USER_NOT_FOUND", "Usuario no encontrado.") { }
    }

    public sealed class UserLockedException : DomainException
    {
        public UserLockedException() : base("USER_LOCKED", "Usuario bloqueado.") { }
    }

    public sealed class InvalidCredentialsException : DomainException
    {
        public InvalidCredentialsException() : base("INVALID_CREDENTIALS", "Credenciales inválidas.") { }
    }

    public sealed class RefreshInvalidException : DomainException
    {
        public RefreshInvalidException() : base("REFRESH_INVALID", "Refresh inválido o expirado.") { }
    }

    // --- Validación genérica (bad request) ---
    public sealed class ValidationException : DomainException
    {
        public ValidationException(string message)
            : base("BAD_REQUEST", message) { }
    }
}
