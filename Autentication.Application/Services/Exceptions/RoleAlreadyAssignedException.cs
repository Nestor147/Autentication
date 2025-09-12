using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Application.Services.Exceptions
{
    public sealed class RoleAlreadyAssignedException : Exception
    {
        public int UserId { get; }
        public int RoleId { get; }
        public RoleAlreadyAssignedException(int userId, int roleId)
            : base($"El rol {roleId} ya está asignado al usuario {userId}.")
        {
            UserId = userId;
            RoleId = roleId;
        }
    }


}
