using Microsoft.EntityFrameworkCore;

namespace Autentication.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //public virtual DbSet<Pais> Paises { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new PaisConfiguration());
  



            base.OnModelCreating(modelBuilder);
            // Aquí mapeos personalizados
        }
    }
}
