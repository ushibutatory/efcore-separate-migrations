using Microsoft.EntityFrameworkCore;
using Sample.Data.Entities;

namespace Sample.Data
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options) { }

        public DbSet<Dog> Dogs { get; set; }
    }
}
