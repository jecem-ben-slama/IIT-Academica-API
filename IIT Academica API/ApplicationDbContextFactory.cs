using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

// This class explicitly tells the EF Core tooling (Add-Migration, Update-Database, etc.) 
// how to instantiate the ApplicationDbContext when running commands.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // 1. Set up configuration to load connection string from appsettings.json
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Get the connection string. MAKE SURE this key matches your appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // 3. Configure DbContextOptions
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // IMPORTANT: Change 'SqlServer' if you are using a different provider (e.g., PostgreSql, MySql)
        builder.UseSqlServer(connectionString);

        // 4. Return a new instance of your DbContext
        return new ApplicationDbContext(builder.Options);
    }
}