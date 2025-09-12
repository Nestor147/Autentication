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
using Autentication.Web.Middlewares;
using Autentication.Web.Security; // si tienes atributos/filters aquí (p.ej., InternalOnly/ValidationFilter)
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Infra (DbContext, UoW, repos, etc. – lo que ya registras dentro)
builder.Services.AddInfrastructure(builder.Configuration);

// 2) Llaves (passphrase/kid/bits) desde ENV o appsettings
var passphrase = Environment.GetEnvironmentVariable("AUTH_KEY_PASSPHRASE")
                 ?? builder.Configuration["Auth:KeyPassphrase"]
                 ?? throw new InvalidOperationException("Falta AUTH_KEY_PASSPHRASE o Auth:KeyPassphrase");

var kid = builder.Configuration["Auth:Kid"] ?? "k1";
var rsaBits = int.TryParse(builder.Configuration["Auth:RsaBits"], out var bits) ? bits : 2048;

// 3) Carga o crea llaves y expone emisor de JWT
var keyDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
var keys = KeyStore.LoadOrCreate(passphrase, keyDir, kid: kid, rsaBits: rsaBits);

builder.Services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddHttpContextAccessor();

// 4) Controllers + (opcional) filtro de validación global
builder.Services.AddControllers(options =>
{
    // Descomenta si creaste el filtro de validación:
    // options.Filters.Add<ValidationFilter>();
})
// Opcional: JSON camelCase y enums como string
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// 5) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
});

// 6) CORS (abrir según tu necesidad)
//   IMPORTANTE: Para llamadas server-to-server no necesitas CORS.
//   Mantener abierto si probarás desde Swagger u orígenes varios.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetPreflightMaxAge(TimeSpan.FromDays(1))
    );
});

// 7) Middleware global de excepciones (ApiError uniforme)
builder.Services.AddTransient<ApiExceptionMiddleware>();

// // 8) (Opcional) Autenticación/JWT si vas a proteger endpoints con [Authorize]
// builder.Services.AddAuthentication(/* esquema */);
// builder.Services.AddAuthorization(/* policies */);

var app = builder.Build();

// 9) Developer exception page (solo si quieres ver stack en dev).
//    Si lo dejas activo, en Development verás la página de error de ASP.NET
//    en lugar de tu formato ApiError. Puedes comentarlo para unificar.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// 10) Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
});

// 11) Orden del pipeline
app.UseRouting();

// ?? Tu middleware global de errores (antes de CORS/Auth y de MapControllers)
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseCors("AppCors");
app.UseHttpsRedirection();

// // Si activas JWT/Policies en controladores:
// app.UseAuthentication();
// app.UseAuthorization();

// 12) Endpoints
app.MapControllers();
app.MapGet("/", () => "Auth API OK");

app.Run();


// -------------- Tip: clases usadas --------------
// Asegúrate de tener estas clases en tu proyecto:
//
// - ApiExceptionMiddleware  (mapea DomainException/401/500 -> ApiError JSON)
// - DomainException y tus excepciones concretas (UserDuplicateException, etc.)
// - ApiOk<T> / ApiError (contratos de respuesta uniformes)
// - KeyStore.LoadOrCreate(...) (tu util para llaves RSA)
// - RsaJwtIssuer (emisor de tokens con tu private.pem)
