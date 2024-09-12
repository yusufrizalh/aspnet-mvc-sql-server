using Microsoft.EntityFrameworkCore;
using MyWebMvc.Models;

namespace MyWebMvc.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
    }
}