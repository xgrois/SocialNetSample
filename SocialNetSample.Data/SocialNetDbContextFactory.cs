using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SocialNetSample.Data;

public class SocialNetDbContextFactory : IDesignTimeDbContextFactory<SocialNetDbContext>
{
    public SocialNetDbContext CreateDbContext(string[]? args = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SocialNetDbContext>();
        optionsBuilder
            //.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .UseSqlServer(configuration.GetConnectionString("Default"));
        //.UseSqlite("Data Source=blog.db");

        return new SocialNetDbContext(optionsBuilder.Options);
    }
}
