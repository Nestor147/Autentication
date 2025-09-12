using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autentication.Core.Entities.Core
{
    public sealed class ApiError
    {
        public bool Success { get; init; } = false;
        public string Code { get; init; } = default!;    // p.ej. USER_DUPLICATE
        public string Message { get; init; } = default!; // legible pero neutral
        public object? Details { get; init; }
        public string? CorrelationId { get; init; }
    }
}
