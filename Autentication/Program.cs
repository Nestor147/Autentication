//using Autentication.Application.Interfaces.Jwt;
//using Autentication.Application.Jwt;
//using Autentication.Application.Password;
//using Autentication.Infrastructure.DependencyInjection;
//using Autentication.Web.Security;

//var builder = WebApplication.CreateBuilder(args);

//// 1) Infra
//builder.Services.AddInfrastructure(builder.Configuration);

//// 2) Keys config
//var passphrase = Environment.GetEnvironmentVariable("AUTH_KEY_PASSPHRASE")
//                 ?? builder.Configuration["Auth:KeyPassphrase"]
//                 ?? throw new InvalidOperationException("Falta AUTH_KEY_PASSPHRASE o Auth:KeyPassphrase");

//var kid = builder.Configuration["Auth:Kid"] ?? "k1";
//var rsaBits = int.TryParse(builder.Configuration["Auth:RsaBits"], out var bits) ? bits : 2048;

//// 3) Load/Create keys
//var keyDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
//var keys = KeyStore.LoadOrCreate(passphrase, keyDir, kid: kid, rsaBits: rsaBits);

//// 4) Security services
//builder.Services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
//builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
//builder.Services.AddHttpContextAccessor();

//// 5) Web + Swagger
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// 6) CORS ABIERTO (permite cualquier origen)
//// ? No usar AllowCredentials con AllowAnyOrigin
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AppCors", p => p
//        .AllowAnyOrigin()
//        .AllowAnyHeader()
//        .AllowAnyMethod()
//        .SetPreflightMaxAge(TimeSpan.FromDays(1))
//    );
//});

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

//app.UseSwagger();
//app.UseSwaggerUI();

//// Orden del middleware importa
//app.UseRouting();
//app.UseCors("AppCors");     // antes de auth/autz y de MapControllers
//app.UseHttpsRedirection();
//// app.UseAuthentication(); // si activas validación JWT
//// app.UseAuthorization();

//app.MapControllers();
//app.MapGet("/", () => "Auth API OK");

//app.Run();
using Autentication.Application.Interfaces.Jwt;
using Autentication.Application.Jwt;
using Autentication.Application.Password;
using Autentication.Infrastructure.DependencyInjection;
using Autentication.Web.Middlewares; // ApiExceptionMiddleware
using Autentication.Web.Security;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Infraestructura (DbContext, UoW, repos, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// 2) Claves/JWT (leer passphrase/kid/bits desde ENV o appsettings)
var passphrase = Environment.GetEnvironmentVariable("AUTH_KEY_PASSPHRASE")
                 ?? builder.Configuration["Auth:KeyPassphrase"]
                 ?? throw new InvalidOperationException("Falta AUTH_KEY_PASSPHRASE o Auth:KeyPassphrase");

var kid = builder.Configuration["Auth:Kid"] ?? "k1";
var rsaBits = int.TryParse(builder.Configuration["Auth:RsaBits"], out var bits) ? bits : 2048;

// 3) Crea/emite llaves y emisor JWT
var keyDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
var keys = KeyStore.LoadOrCreate(passphrase, keyDir, kid: kid, rsaBits: rsaBits);

builder.Services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddHttpContextAccessor();

// 4) Controllers (JSON en camelCase; NO mapeo automático de ProblemDetails de cliente si quieres evitar sorpresas)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(o =>
    {
        // Opcional: si no quieres que ASP.NET genere ProblemDetails 4xx/5xx automáticos.
        o.SuppressMapClientErrors = true;
    });

// 5) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
});

// 6) CORS (ajusta según necesidad)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetPreflightMaxAge(TimeSpan.FromDays(1))
    );
});

// 7) Registra el middleware de excepciones (se usará solo en ciertas rutas)
builder.Services.AddTransient<ApiExceptionMiddleware>();

// 8) (Opcional) Autenticación/Autorización si usas [Authorize]
// builder.Services.AddAuthentication(/* esquema */);
// builder.Services.AddAuthorization(/* policies */);

var app = builder.Build();

// ?? No uses DeveloperExceptionPage si quieres respuestas uniformes en tus tres controladores
// if (app.Environment.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

// --- RUTAS que SÍ tendrán manejo de errores con ApiExceptionMiddleware ---
// REEMPLAZA estas 3 cadenas por tus rutas base reales (de esos 3 controladores):
var handledPrefixes = new[]
{
    "/api/auth",        // p.ej. AuthController
    "/api/users",       // p.ej. UsersController
    "/api/admins"       // p.ej. AdminsController
};

// 9) Usa el middleware de excepciones SOLO cuando el Path coincida con tus prefijos
app.UseWhen(
    ctx => handledPrefixes.Any(p =>
        ctx.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)),
    branch =>
    {
        // Ponlo al INICIO del branch para envolver todo lo que sigue
        branch.UseMiddleware<ApiExceptionMiddleware>();
    }
);

// 10) El resto del pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
});

app.UseRouting();

app.UseCors("AppCors");
app.UseHttpsRedirection();

// Si activas JWT/Policies en controladores:
// app.UseAuthentication();
// app.UseAuthorization();

// 11) Endpoints
app.MapControllers();
app.MapGet("/", () => "Auth API OK");

app.Run();
