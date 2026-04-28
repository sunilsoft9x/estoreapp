namespace MyEstore.Data;
using Microsoft.EntityFrameworkCore;
using MyEstore.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }
    //Dbsets or Tables for each model
    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<ProductModel> Products { get; set; }
    public DbSet<CategoryModel> Categories { get; set; }
    public DbSet<CartModel> Carts { get; set; }
    public DbSet<CartItemModel> CartItems { get; set; }
    public DbSet<OrderModel> Orders { get; set; }
    public DbSet<OrderItemModel> OrderItems { get; set; }
    public DbSet<PaymentModel> Payments { get; set; }
    public DbSet<OtpVerificationModel> OtpVerifications { get; set; }
    public DbSet<RefreshTokenModel> RefreshTokens { get; set; }
    public DbSet<WishlistItemModel> WishlistItems { get; set; }
    public DbSet<CouponModel> Coupons { get; set; }
    public DbSet<CouponUsageModel> CouponUsages { get; set; }
    //Relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //User-Role Many to One
        modelBuilder.Entity<UserModel>()
            .HasOne(u => u.Role)
            .WithMany(r=> r.Users)
            .HasForeignKey(u=> u.RoleId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent role deletion if users exist
            
            //Product-Category Many to One
            modelBuilder.Entity<ProductModel>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent category deletion if products exist
    //Cart-User One to One
        modelBuilder.Entity<CartModel>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<CartModel>(c=>c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete cart if user is deleted
        modelBuilder.Entity<CartItemModel>()
            .HasOne(ci => ci.Cart)
            .WithMany(c=> c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade); // Delete cart items if cart is deleted`
        modelBuilder.Entity<CartItemModel>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent product deletion if in cart

            modelBuilder.Entity<OrderModel>()
            .HasOne(o => o.User)
            .WithMany(u =>u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if orders exist
        modelBuilder.Entity<OrderItemModel>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Delete order items if order is deleted
        modelBuilder.Entity<OrderItemModel>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent product deletion if in order
            //Payment-Order
        modelBuilder.Entity<PaymentModel>()
            .HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade); // Delete payment if order is deleted
    //Constraints and Indexes
        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique(); // Unique constraint on Email
        modelBuilder.Entity<RoleModel>()
            .HasIndex(r => r.Name)
            .IsUnique(); // Unique constraint on Role Name
        modelBuilder.Entity<OrderModel>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique(); // Unique constraint on Order Number

        // RefreshToken → User
        modelBuilder.Entity<RefreshTokenModel>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WishlistItem → User + Product (unique per pair)
        modelBuilder.Entity<WishlistItemModel>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<WishlistItemModel>()
            .HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<WishlistItemModel>()
            .HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique();

        // Coupon unique code index
        modelBuilder.Entity<CouponModel>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // CouponUsage → Coupon
        modelBuilder.Entity<CouponUsageModel>()
            .HasOne(cu => cu.Coupon)
            .WithMany(c => c.Usages)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order → Coupon (nullable FK)
        modelBuilder.Entity<OrderModel>()
            .HasOne(o => o.Coupon)
            .WithMany()
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}