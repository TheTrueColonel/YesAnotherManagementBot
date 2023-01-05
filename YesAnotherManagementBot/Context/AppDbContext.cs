using Microsoft.EntityFrameworkCore;
using YAMB.Context.Models;

namespace YAMB.Context; 

public class AppDbContext : DbContext {
    public AppDbContext() { }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options) { }
    
    public virtual DbSet<UserBans> UserBans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            optionsBuilder.UseSqlServer(GlobalSettings.Instance.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserBans>(entity => {
            entity.HasKey(e => e.BanId)
                  .HasName("UserBans_pk");

            entity.HasIndex(e => e.BanId)
                  .HasDatabaseName("UserBans_BanId_uindex")
                  .IsUnique();

            entity.Property(e => e.BanId)
                  .IsRequired()
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.UserId)
                  .IsRequired();

            entity.Property(e => e.BannedAt)
                  .IsRequired();
        });
    }
}