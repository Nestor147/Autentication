using Autentication.Application.Interfaces.Jwt;
using Autentication.Application.Jwt;
using Autentication.Application.Password;
using Autentication.Infrastructure.DependencyInjection;
using Autentication.Web.Security;


var builder = WebApplication.CreateBuilder(args);

// 0) Origins permitidos (appsettings o var de entorno)
var corsOrigins = (builder.Configuration["Cors:Origins"] ?? "http://localhost:4200")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// 1) Infra
builder.Services.AddInfrastructure(builder.Configuration);

// 2) Keys config
var passphrase = Environment.GetEnvironmentVariable("AUTH_KEY_PASSPHRASE")
                 ?? builder.Configuration["Auth:KeyPassphrase"]
                 ?? throw new InvalidOperationException("Falta AUTH_KEY_PASSPHRASE o Auth:KeyPassphrase");

var kid = builder.Configuration["Auth:Kid"] ?? "k1";
var rsaBits = int.TryParse(builder.Configuration["Auth:RsaBits"], out var bits) ? bits : 2048;

// 3) Load/Create keys
var keyDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
var keys = KeyStore.LoadOrCreate(passphrase, keyDir, kid: kid, rsaBits: rsaBits);

// 4) Security services
builder.Services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddHttpContextAccessor();

// 5) Web + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 6) ? CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", p => p
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        // .AllowCredentials() // solo si usas cookies
        .SetPreflightMaxAge(TimeSpan.FromDays(1))
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();

// ?? Orden del middleware importa
app.UseRouting();
app.UseCors("AppCors");          // ? antes de auth/autz y antes de MapControllers
app.UseHttpsRedirection();
// app.UseAuthentication();      // si tu paquete valida JWT, actívalo
// app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Auth API OK");

app.Run();
