namespace WebApi.Helpers;

using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using BCrypt.Net;

public class DataContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    
    private readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasData(
            new Account
            {
                Id = 1,
                Title = "Test01",
                FirstName = "Test",
                LastName = "01",

                Email = "test01@a.net",
                PasswordHash = BCrypt.HashPassword("1111"),
                Role = Role.Admin,
                VerificationToken = String.Empty,
                Verified = DateTime.Now,
                PasswordReset = DateTime.Now,
            });
    }
}