using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Oracle.EntityFrameworkCore;

namespace Infra.Context
{
    public class IdentityDbContextFactory
        : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "..",
                        "Helpdesk"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                configuration.GetConnectionString("ORACLE");

            var optionsBuilder =
                new DbContextOptionsBuilder<IdentityContext>();

            optionsBuilder.UseOracle(
                connectionString,
                oracleOptions =>
                {
                    oracleOptions.UseOracleSQLCompatibility(
                        OracleSQLCompatibility.DatabaseVersion19);
                });

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}