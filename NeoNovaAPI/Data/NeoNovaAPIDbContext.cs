using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.DbModels;
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
        public DbSet<StoreHour> StoreHours { get; set; } = default!;
        public DbSet<Novadeck> Novadecks { get; set; } = default!;
    }
}
