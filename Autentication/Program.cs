using Autentication.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Inyecci�n de dependencias
builder.Services.AddInfrastructure(builder.Configuration);

// Servicios base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ? CORS debe ir aqu�, ANTES del build
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware HTTP global
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend Atacado API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// ? Aplica CORS antes de Authorization
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "? API Backend Atacado en l�nea");

app.Run();
