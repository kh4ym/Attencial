using Attencial.API.Models;
using Microsoft.EntityFrameworkCore;



namespace Attencial.API.Datal;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<User> Users { get; set; }
    }