using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infraestructure.Db
{
    public class Context : DbContext
    {
        private readonly IConfiguration _configurationAppSettings;
        public Context(IConfiguration configurationAppSettings)
        {
            _configurationAppSettings = configurationAppSettings;
        }
        public DbSet<Administrador> administradores { get; set; } = default!;
        public DbSet<Veiculo> veiculos { get; set; } = default!;    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrador>().HasData(
                new Administrador
                {
                    Id = 1,
                    Email = "Administrador@teste.com",
                    Password = "12345",
                    Profile = "Adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conectionStrng = _configurationAppSettings.GetConnectionString("SQLServer")?.ToString();
                if (!String.IsNullOrEmpty(conectionStrng))
                {
                    optionsBuilder.UseSqlServer(conectionStrng);
                };
            }
        }
    }
}
