using beautix_bisp_17005.Models.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Data
{
    /// <summary>
    /// The Entity Framework Core "bridge" between our C# objects and the PostgreSQL
    /// database. It inherits from IdentityDbContext so all the Identity tables
    /// (users, roles, claims, logins) are created and managed automatically, and it
    /// implements IDataProtectionKeyContext so the app's encryption keys are stored
    /// in the database too (so they survive restarts/redeploys on Railway).
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Each DbSet becomes a table. These are the "entry points" we query in the
        // service classes (e.g. _context.Bookings.Where(...)).
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Salon> Salons { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        // Stores the ASP.NET DataProtection keys used to encrypt auth cookies.
        public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; }

        /// <summary>
        /// Fluent API configuration — this is where we tell EF Core exactly how the
        /// model maps to database columns, the foreign-key relationships, the
        /// delete behaviours, and any starter data to seed.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- Column types -------------------------------------------------
            // Money is mapped to numeric(18,2) so we get exact decimal precision
            // (floating point would introduce rounding errors on prices).
            builder.Entity<SubscriptionPlan>()
                .Property(p => p.MonthlyPrice)
                .HasColumnType("numeric(18,2)");

            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("numeric(18,2)");

            // Boolean column types — required for PostgreSQL seed data
            builder.Entity<SubscriptionPlan>()
                .Property(p => p.IsActive)
                .HasColumnType("boolean");

            builder.Entity<Salon>()
                .Property(s => s.IsApproved)
                .HasColumnType("boolean");

            builder.Entity<Service>()
                .Property(s => s.IsAvailable)
                .HasColumnType("boolean");

            builder.Entity<UserSubscription>()
                .Property(us => us.IsActive)
                .HasColumnType("boolean");

            // --- Relationships & delete rules ---------------------------------
            // The OnDelete behaviour is the interesting part to explain: it decides
            // what happens to child rows when a parent is deleted.

            // If a user is deleted, delete their subscriptions too (Cascade).
            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Don't allow deleting a plan that subscriptions still point to
            // (Restrict) — this protects historical/billing data.
            builder.Entity<UserSubscription>()
                .HasOne(us => us.Plan)
                .WithMany(p => p.UserSubscriptions)
                .HasForeignKey(us => us.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // If a user is deleted, remove their bookings too (Cascade).
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // If a service is deleted, keep the booking for history but null out
            // the link (SetNull) — that's why Booking.ServiceId is nullable.
            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // A salon points to its owner; Restrict stops an owner being deleted
            // while they still own a salon.
            builder.Entity<Salon>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Deleting a salon cascades to remove all of its services.
            builder.Entity<Salon>()
                .HasMany(s => s.Services)
                .WithOne(sv => sv.Salon)
                .HasForeignKey(sv => sv.SalonId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- Seed data ----------------------------------------------------
            // The three subscription tiers are inserted automatically by the
            // migration, so the catalogue is ready the first time the app runs.
            // (999 allowance = "unlimited", per the SubscriptionService convention.)
            builder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    Id = 1,
                    Name = "Basic",
                    Description = "3 services per month",
                    MonthlyPrice = 79,
                    ServiceAllowance = 3,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Id = 2,
                    Name = "Standard",
                    Description = "5 services per month",
                    MonthlyPrice = 100,
                    ServiceAllowance = 5,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Id = 3,
                    Name = "Premium",
                    Description = "Unlimited services per month",
                    MonthlyPrice = 130,
                    ServiceAllowance = 999,
                    IsActive = true
                }
            );
        }
    }
}