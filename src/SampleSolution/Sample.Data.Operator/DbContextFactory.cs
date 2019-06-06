using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sample.Data.Operator
{
    public class DbContextFactory : IDesignTimeDbContextFactory<DbContext>
    {
        public DbContext CreateDbContext(string[] args)
        {
            // NOTE: appsettings.jsonから読み込む場合
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connectionString = config.GetConnectionString("Default");

            // NOTE: べた書きする場合
            //var connectionString = "Server=...";

            // NOTE: MigrationsAssemblyでMigrationファイルを作成するアセンブリを指定する
            var builder = new DbContextOptionsBuilder<DbContext>()
                .UseSqlServer(connectionString, _ => _.MigrationsAssembly(typeof(DbContextFactory).Namespace));

            return new DbContext(builder.Options);
        }
    }
}
