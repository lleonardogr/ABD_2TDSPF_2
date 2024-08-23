using CardGameManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardGameManagement.Data;

public class CardGameMetadataDbContext : DbContext
{
    public DbSet<Card> Cards { get; set; }
    public DbSet<Set> Sets { get; set; }

    public CardGameMetadataDbContext(DbContextOptions<CardGameMetadataDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data for Sets
        modelBuilder.Entity<Set>().HasData(
            new Set { Id = 1, Name = "Alpha" },
            new Set { Id = 2, Name = "Beta" }
        );

        // Seed data for Cards
        modelBuilder.Entity<Card>().HasData(
            new Card { Id = 1, Name = "Black Lotus", SetId = 1 },
            new Card { Id = 2, Name = "Ancestral Recall", SetId = 1 },
            new Card { Id = 3, Name = "Time Walk", SetId = 2 }
        );
    }
    
    
}