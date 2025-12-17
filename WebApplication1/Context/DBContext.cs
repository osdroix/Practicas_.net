
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

    public class DBContext : DbContext
{
    public DBContext(DbContextOptions options)
        :base(options)
    {
        }
        public DbSet<TaksItem> TaksItems { get; set; }
}

