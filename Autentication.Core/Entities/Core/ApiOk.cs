using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Core
{
    public sealed class ApiOk<T>
    {
        public bool Success { get; init; } = true;
        public string Code { get; init; } = "OK";
        public T Data { get; init; } = default!;
    }
}
