using Chat.Minimal.Services.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chat.Minimal.Services.Infrastructure;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQL Server for design-time migrations
        var connectionString = "Data Source=websql3.internetbrasil.net,1433;Initial Catalog=freetecnologia;Persist Security Info=True;User ID=freetecnologia;Password=Xz4O2]JSx5ms;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=Chat.Minimal.Services;Command Timeout=0";
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

}


