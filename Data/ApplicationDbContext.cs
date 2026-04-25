using beautix_bisp_17005.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace beautix_bisp_17005.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Salon> Salons { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SubscriptionPlan>()
                .Property(p => p.MonthlyPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSubscription>()
                .HasOne(us => us.Plan)
                .WithMany(p => p.UserSubscriptions)
                .HasForeignKey(us => us.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);                          

            builder.Entity<Salon>()
                .HasOne(s => s.Owner)
                .WithMany()
                .HasForeignKey(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Salon>()
                .HasMany(s => s.Services)
                .WithOne(sv => sv.Salon)
                .HasForeignKey(sv => sv.SalonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { Id = 1, Name = "Basic", Description = "3 services per month", MonthlyPrice = 79, ServiceAllowance = 3, IsActive = true },
                new SubscriptionPlan { Id = 2, Name = "Standard", Description = "5 services per month", MonthlyPrice = 100, ServiceAllowance = 5, IsActive = true },
                new SubscriptionPlan { Id = 3, Name = "Premium", Description = "Unlimited services per month", MonthlyPrice = 130, ServiceAllowance = 999, IsActive = true }
            );
        }
    }
}