using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.DbModels;
using NeoNovaAPI.Models.UserModels;
using NeoNovaAPI.Models.WholesaleModels;
using System;

namespace NeoNovaAPI.Data
{
    public class NeoNovaAPIDbContext : IdentityDbContext<IdentityUser>
    {
        public NeoNovaAPIDbContext(DbContextOptions<NeoNovaAPIDbContext> options) : base(options)
        {
        }
        public DbSet<Faq> Faqs { get; set; } = default!;
        public DbSet<Geofence> Geofences { get; set; } = default!;
        public DbSet<Store> Stores { get; set; } = default!;
        public DbSet<Novadeck> Novadecks { get; set; } = default!;
        public DbSet<WholesaleBugMessage> WholesaleBugMessages { get; set; } = default!;

        public DbSet<SecurityUser> SecurityUsers { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SecurityUser>()
                .HasOne(s => s.IdentityUser)
                .WithOne()
                .HasForeignKey<SecurityUser>(s => s.IdentityUserId);
        }


        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is WholesaleBugMessage && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    ((WholesaleBugMessage)entityEntry.Entity).CreatedAt = DateTime.Now;
                }

                ((WholesaleBugMessage)entityEntry.Entity).ModifiedAt = DateTime.Now;
            }

            return base.SaveChanges();
        }

    }
}
