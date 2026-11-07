using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Oracle.EntityFrameworkCore;

namespace Infra.Context
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "..",
                        "Helpdesk"))
                .AddJsonFile(
                    "appsettings.json",
                    optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                configuration.GetConnectionString("ORACLE");

            var optionsBuilder =
                new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseOracle(
                connectionString,
                oracleOptions =>
                {
                    oracleOptions.UseOracleSQLCompatibility(
                        OracleSQLCompatibility.DatabaseVersion19);
                });

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}