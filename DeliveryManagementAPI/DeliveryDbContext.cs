using Microsoft.EntityFrameworkCore;
using DeliveryManagementAPI.Models;

namespace DeliveryManagementAPI
{
    public class DeliveryDbContext : DbContext
    {
        public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DeliveryStaff> DeliveryStaffs { get; set; }
        public DbSet<LocationCheckpoint> LocationCheckpoints { get; set; }
        public DbSet<Customer> Customers { get; set; }

    public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for Order properties
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.CollectionAmount)
                .HasPrecision(18, 2);
        }
    }
}
