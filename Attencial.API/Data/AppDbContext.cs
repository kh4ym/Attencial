using Attencial.API.Models;
using Microsoft.EntityFrameworkCore;



namespace Attencial.API.Data;
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<User> Users { get; set; }
    }