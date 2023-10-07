﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.DbModels;
using NeoNovaAPI.Models.Loyalty;
using NeoNovaAPI.Models.SecurityModels.Archiving;
using NeoNovaAPI.Models.SecurityModels.CameraManagement;
using NeoNovaAPI.Models.SecurityModels.Chat;
using NeoNovaAPI.Models.SecurityModels.Reporting;
using NeoNovaAPI.Models.SecurityModels.ShiftManagement;
using NeoNovaAPI.Models.SecurityModels.TourManagement;
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
        // Mobile App DB Sets
        public DbSet<Faq> Faqs { get; set; } = default!;
        public DbSet<Geofence> Geofences { get; set; } = default!;
        public DbSet<Store> Stores { get; set; } = default!;
        public DbSet<Novadeck> Novadecks { get; set; } = default!;

        // Wholesale DB Sets
        public DbSet<WholesaleBugMessage> WholesaleBugMessages { get; set; } = default!;

        // Security DB Sets
        public DbSet<SecurityUser> SecurityUsers { get; set; }
        public DbSet<Archive> Archives { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraHistory> CameraHistories { get; set; }
        public DbSet<CameraLocation> CameraLocations { get; set; }
        public DbSet<CameraStatus> CameraStatuses { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ChatLog> ChatLogs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Note> Notes { get; set; } = default!;
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Tour> Tours { get; set; }

        // Loyalty DB Sets
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; } = default!;
        public DbSet<UserLoyaltyStatus> UserLoyaltyStatuses { get; set; } = default!;
        public DbSet<LoyaltyTier> LoyaltyTiers { get; set; } = default!;
        public DbSet<SmallPerk> SmallPerks { get; set; } = default!;
        public DbSet<BigPerk> BigPerks { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SecurityUser>()
                .HasOne(s => s.IdentityUser)
                .WithOne()
                .HasForeignKey<SecurityUser>(s => s.IdentityUserId);

            modelBuilder.Entity<CameraLocation>()
                .HasKey(cl => new { cl.LocationId, cl.CameraId });

            modelBuilder.Entity<CameraLocation>()
                .HasOne(cl => cl.Location)
                .WithMany(l => l.CameraLocations)
                .HasForeignKey(cl => cl.LocationId);


            // For LoyaltyProgram and LoyaltyTier
            modelBuilder.Entity<LoyaltyProgram>()
                .HasMany(lp => lp.Tiers)
                .WithOne(t => t.LoyaltyProgram)
                .HasForeignKey(t => t.LoyaltyProgramId);

            // For UserLoyaltyStatus and LoyaltyTier
            modelBuilder.Entity<UserLoyaltyStatus>()
                .HasOne(uls => uls.CurrentTier)
                .WithMany()
                .HasForeignKey(uls => uls.CurrentTierId);

            // For AspNetUsers and UserLoyaltyStatus
            //modelBuilder.Entity<AspNetUsers>()
                //.HasOne(a => a.UserLoyaltyStatus)
                //.WithMany()
                //.HasForeignKey(a => a.UserLoyaltyStatusId);

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
