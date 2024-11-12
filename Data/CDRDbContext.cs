using Microsoft.EntityFrameworkCore;
using CDRProcessingAPI.Models;

namespace CDRProcessingAPI.Data
{
    public class CDRDbContext : DbContext
    {
        public CDRDbContext(DbContextOptions<CDRDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CDR> CDRs { get; set; }
    }
}
