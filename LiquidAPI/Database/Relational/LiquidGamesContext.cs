using LiquidAPI.Relational;
using Microsoft.EntityFrameworkCore;

namespace LiquidAPI.Database.Relational;

public class LiquidGamesContext : DbContext
{
    public LiquidGamesContext(DbContextOptions<LiquidGamesContext> options,IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<Platform> Platforms { get; set; }

    private readonly IConfiguration _configuration;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Postgres"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Genre
        modelBuilder.Entity<Genre>()
            .HasKey(g => g.GenreId);

        // Game
        modelBuilder.Entity<Game>()
            .HasKey(g => g.GameId);
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Genre)
            .WithMany(g => g.Games)
            .HasForeignKey(g => g.GenreId);
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Publisher)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PublisherId);
        modelBuilder.Entity<Game>()
            .HasOne(g => g.Platform)
            .WithMany(p => p.Games)
            .HasForeignKey(g => g.PlatformId);

        // Publisher
        modelBuilder.Entity<Publisher>()
            .HasKey(p => p.PublisherId);

        // Platform
        modelBuilder.Entity<Platform>()
            .HasKey(p => p.PlatformId);
    }
}
