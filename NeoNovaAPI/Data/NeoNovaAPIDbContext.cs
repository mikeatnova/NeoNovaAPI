using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.DbModels;
using NeoNovaAPI.Models.SecurityModels.Archiving;
using NeoNovaAPI.Models.SecurityModels.CameraManagment;
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
        public DbSet<ShiftNote> ShiftNotes { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourNote> TourNotes { get; set; }

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

            modelBuilder.Entity<CameraLocation>()
                .HasOne(cl => cl.Camera)
                .WithMany(c => c.CameraLocations)
                .HasForeignKey(cl => cl.CameraId);

            modelBuilder.Entity<ShiftNote>()
                .HasOne(sn => sn.Shift)
                .WithMany(s => s.ShiftNotes)
                .HasForeignKey(sn => sn.ShiftId);

            modelBuilder.Entity<TourNote>()
                .HasOne(tn => tn.Tour)
                .WithMany(t => t.TourNotes)
                .HasForeignKey(tn => tn.TourId);

            modelBuilder.Entity<ShiftNote>()
                .HasOne(sn => sn.Note)
                .WithMany()
                .HasForeignKey(sn => sn.NoteId);

            modelBuilder.Entity<TourNote>()
                .HasOne(tn => tn.Note)
                .WithMany()
                .HasForeignKey(tn => tn.NoteId);
            modelBuilder.Entity<CameraStatus>()
                .HasOne(cs => cs.Note)
                .WithMany()
                .HasForeignKey(cs => cs.NoteId);

            modelBuilder.Entity<CameraHistory>()
                .HasOne(ch => ch.Note)
                .WithMany()
                .HasForeignKey(ch => ch.NoteId);

            modelBuilder.Entity<Camera>()
                .HasOne(ch => ch.Note)
                .WithMany()
                .HasForeignKey(ch => ch.NoteId);
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
