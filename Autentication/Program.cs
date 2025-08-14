using Autentication.Infrastructure.DependencyInjection; // <- tu DI (DbContext, UoW, repos)
using Autentication.Web.Security;                       // <- KeyStore
using MyCompany.Security.Jwt;
using MyCompany.Security.Password;

var builder = WebApplication.CreateBuilder(args);

// 1) Infraestructura (asegúrate que AddInfrastructure use "DefaultConnection" o cambia el nombre aquí)
builder.Services.AddInfrastructure(builder.Configuration);

// 2) Lee configuración de claves
var passphrase = Environment.GetEnvironmentVariable("AUTH_KEY_PASSPHRASE")
                 ?? builder.Configuration["Auth:KeyPassphrase"]
                 ?? throw new InvalidOperationException("Falta AUTH_KEY_PASSPHRASE o Auth:KeyPassphrase");

var kid = builder.Configuration["Auth:Kid"] ?? "k1";
var rsaBits = int.TryParse(builder.Configuration["Auth:RsaBits"], out var bits) ? bits : 2048;

// 3) Genera/carga llaves (con logs de error si algo pasa)
var keyDir = Path.Combine(builder.Environment.ContentRootPath, "keys");
KeyStore.KeyPair keys;
try
{
    keys = KeyStore.LoadOrCreate(passphrase, keyDir, kid: kid, rsaBits: rsaBits);
}
catch (Exception ex)
{
    builder.Logging.AddConsole();
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
    logger.LogError(ex, "Error al generar/cargar llaves en {KeyDir}", keyDir);
    throw;
}

// 4) Servicios de seguridad (no duplicar)
builder.Services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddHttpContextAccessor();

// 5) Web API + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Mostrar detalles en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(); // por defecto: /swagger y busca /swagger/v1/swagger.json

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => "Auth API OK");

app.Run();
