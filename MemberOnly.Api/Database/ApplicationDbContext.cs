using MemberOnly.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MemberOnly.Api.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply configurations from the assmebly of the DbContext
        modelBuilder.ApplyConfigurationsFromAssembly( typeof(ApplicationDbContext).Assembly );
        base.OnModelCreating(modelBuilder); 
    }
    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Post> Posts { get; set; } = null!;

    public DbSet<UserRefreshTokens> RefreshTokens { get; set; } = null!;
    
}