using LittleFarmCakes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LittleFarmCakes.Data
{
    public class ApplicationDbContext: IdentityDbContext
        <ApplicationUser, IdentityRole, string,
         IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
         IdentityRoleClaim<string>, IdentityUserToken<string>>
    { 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUserRole> ApplicationUserRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder
        modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                modelBuilder.Entity<Cart>()
              .HasKey(ab => new
              {
                  ab.Id,
                  ab.UserId,
                  ab.ProductId
              });

            modelBuilder.Entity<Cart>()
            .HasOne(ab => ab.Product)
             .WithMany(ab => ab.Carts)
             .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<Cart>()
            .HasOne(ab => ab.User)
           .WithMany(ab => ab.Carts)
           .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<Order>()
            .HasOne(ab => ab.Product)
            .WithMany(ab => ab.Orders)
            .HasForeignKey(ab => ab.ProductId);

            modelBuilder.Entity<Order>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.Orders)
            .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });
        }
    }
}
