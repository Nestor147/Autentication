using Autentication.Application.Interfaces;
using Autentication.Core.Interfaces.Core;
using Autentication.Infrastructure.Context;
using Autentication.Infrastructure.Context.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCompany.Security.Jwt;
using MyCompany.Security.Password;

namespace Autentication.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            //services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
            //services.AddSingleton<IJwtIssuer>(new RsaJwtIssuer(keys.PrivatePem, keyId: keys.Kid));
            //services.AddHttpContextAccessor();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //services.AddScoped<IPaisRepository, PaisRepository>();
            return services;
        }
    }
}
