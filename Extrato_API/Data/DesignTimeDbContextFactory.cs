using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Extrato_API.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            //mesma ordem do Program.cs (DefaultConnection antes de PostgresConnection)
            var connectionString =
                config.GetConnectionString("DefaultConnection") ??
                config.GetConnectionString("PostgresConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Nenhuma connection string configurada para design-time DbContext.");

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}