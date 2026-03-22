using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Models;
using ShoppingCartAPI.Services;

namespace ShoppingCartAPI.Data;

public partial class ShoppingCartContext : DbContext
{
    private readonly EncryptedConverter _encryptedConverter;

    public ShoppingCartContext(DbContextOptions<ShoppingCartContext> options, IEncryptionService encryptionService)
        : base(options)
    {
        _encryptedConverter = new EncryptedConverter(encryptionService);
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<AppConfig> AppConfigs { get; set; }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public virtual  DbSet<Product> Products { get; set; } = null!;
    public virtual DbSet<ProductImage> ProductImages { get; set; } = null!;
    public virtual DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public virtual DbSet<ProductCategoryLink> ProductCategoryLinks { get; set; } = null!;
    public virtual DbSet<Address> Addresses { get; set; } = null!;
    public virtual DbSet<AddressType> AddressTypes { get; set; } = null!;
    public virtual DbSet<Customer> Customers { get; set; } = null!;
    public virtual DbSet<CustomerAddressLink> CustomerAddressLinks { get; set; } = null!;
    public virtual DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
    public virtual DbSet<BillingMethod> BillingMethods { get; set; } = null!;
        public virtual DbSet<BankCardInfo> BankCardInfos { get; set; } = null!;
    public virtual DbSet<CustomerBillingCardLink> CustomerBillingCardLinks { get; set; } = null!;
    public virtual DbSet<ShoppingCartAPI.Models.ShoppingCart> ShoppingCarts { get; set; } = null!;
    public virtual DbSet<ShoppingCartDetail> ShoppingCartDetails { get; set; } = null!;
    public virtual DbSet<ProductImportRecord> ProductImportRecords { get; set; } = null!;
    public virtual DbSet<ProductInventory> ProductInventories { get; set; } = null!;
    public virtual DbSet<Order> Orders { get; set; } = null!;
    public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public virtual DbSet<ShippingServiceProvider> ShippingServiceProviders { get; set; } = null!;
    public virtual DbSet<ShippingTracking> ShippingTrackings { get; set; } = null!;
    public virtual DbSet<ShippingItemDetail> ShippingItemDetails { get; set; } = null!;
    public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
    public virtual DbSet<UserCustomerLink> UserCustomerLinks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductCategory>(entity => {
            entity.ToTable("ProductCategory");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.CategoryName)
                   .IsRequired()
                   .HasMaxLength(200);
            entity.Property(c => c.Description)
                   .HasMaxLength(1000);
            entity.Property(c => c.Created)
                   .IsRequired();
            entity.Property(c => c.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(c => c.Modified);
            entity.Property(c => c.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasData(
                new ProductCategory { Id = 1, CategoryName = "Women", Description = "Women's products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 2, CategoryName = "Men", Description = "Men's products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 3, CategoryName = "Kids", Description = "Kids' products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 4, CategoryName = "Home", Description = "Home products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 5, CategoryName = "Garden", Description = "Garden products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 6, CategoryName = "Kitchen", Description = "Kitchen products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 7, CategoryName = "Backyard", Description = "Backyard products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 8, CategoryName = "Electronic", Description = "Electronic products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 9, CategoryName = "Makeup", Description = "Makeup products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 10, CategoryName = "Office", Description = "Office products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 11, CategoryName = "Bedroom", Description = "Bedroom products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 12, CategoryName = "Family Room", Description = "Family room products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new ProductCategory { Id = 13, CategoryName = "Dinner Room", Description = "Dinner room products", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" }
            );
        });

        modelBuilder.Entity<Product>(entity => {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.ProductName)
                   .IsRequired()
                   .HasMaxLength(200);
            entity.Property(p => p.SKU)
                   .HasMaxLength(50);
            entity.Property(p => p.UPC)
                   .HasMaxLength(12);
            entity.Property(p => p.BrandName)
                   .HasMaxLength(200);
            entity.Property(p => p.Price)
                   .HasColumnType("decimal(18,2)");
            entity.Property(p => p.Description)
                   .HasMaxLength(1000);
            entity.Property(p => p.Created)
                   .IsRequired();
            entity.Property(p => p.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(p => p.Modified);
            entity.Property(p => p.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasMany(p => p.Images)
                   .WithOne(i => i.Product)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductCategoryLink>(entity => {
            entity.ToTable("ProductCategoryLink");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedOnAdd();
            entity.Property(l => l.Created)
                   .IsRequired();
            entity.Property(l => l.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(l => l.Modified);
            entity.Property(l => l.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasIndex(l => new { l.ProductId, l.ProductCategoryId })
                   .IsUnique();

            entity.HasOne(l => l.Product)
                   .WithMany(p => p.ProductCategoryLinks)
                   .HasForeignKey(l => l.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.ProductCategory)
                   .WithMany(c => c.ProductCategoryLinks)
                   .HasForeignKey(l => l.ProductCategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity => {
            entity.ToTable("ProductImages");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);
            entity.Property(i => i.AltText)
                   .HasMaxLength(200);
            entity.Property(i => i.Created)
                   .IsRequired();
            entity.Property(i => i.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(i => i.Modified);
            entity.Property(i => i.ModifiedBy)
                   .HasMaxLength(256);
        });

        modelBuilder.Entity<Address>(entity => {
            entity.ToTable("Address");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();
            entity.Property(a => a.StreetNo)
                   .HasMaxLength(20);
            entity.Property(a => a.Street)
                   .IsRequired()
                   .HasMaxLength(200);
            entity.Property(a => a.City)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(a => a.PostalCode)
                   .IsRequired()
                   .HasMaxLength(20);
            entity.Property(a => a.Province)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(a => a.Country)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(a => a.Created)
                   .IsRequired();
            entity.Property(a => a.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(a => a.Modified);
            entity.Property(a => a.ModifiedBy)
                   .HasMaxLength(256);
        });

        modelBuilder.Entity<AddressType>(entity => {
            entity.ToTable("AddressType");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.Property(t => t.TypeName)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(t => t.Description)
                   .HasMaxLength(500);
            entity.Property(t => t.Created)
                   .IsRequired();
            entity.Property(t => t.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(t => t.Modified);
            entity.Property(t => t.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasData(
                new AddressType { Id = 1, TypeName = "BillingAddress", Description = "Billing address", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new AddressType { Id = 2, TypeName = "ShippingAddress", Description = "Shipping address", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new AddressType { Id = 3, TypeName = "BillingAndShippingAddress", Description = "Billing and shipping address", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" }
            );
        });

        modelBuilder.Entity<Customer>(entity => {
            entity.ToTable("Customer");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedOnAdd();
            entity.Property(c => c.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(c => c.MiddleName)
                   .HasMaxLength(100);
            entity.Property(c => c.LastName)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(c => c.Email)
                   .HasMaxLength(256);
            entity.Property(c => c.PhoneNumber)
                   .HasMaxLength(20);
            entity.Property(c => c.Created)
                   .IsRequired();
            entity.Property(c => c.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(c => c.Modified);
            entity.Property(c => c.ModifiedBy)
                   .HasMaxLength(256);
        });

        modelBuilder.Entity<CustomerAddressLink>(entity => {
            entity.ToTable("CustomerAddressLink");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedOnAdd();
            entity.Property(l => l.Created)
                   .IsRequired();
            entity.Property(l => l.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(l => l.Modified);
            entity.Property(l => l.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(l => l.Customer)
                   .WithMany(c => c.CustomerAddressLinks)
                   .HasForeignKey(l => l.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Address)
                   .WithMany(a => a.CustomerAddressLinks)
                   .HasForeignKey(l => l.AddressId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.AddressType)
                   .WithMany(t => t.CustomerAddressLinks)
                   .HasForeignKey(l => l.AddressTypeId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>()
                 .HasIndex(rt => rt.TokenHash)
                 .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);



        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasData(
                new Role { Id = 1, RoleName = "ADMIN", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new Role { Id = 2, RoleName = "USER", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new Role { Id = 3, RoleName = "ExternalClient", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" }
            );
        });

        modelBuilder.Entity<OrderStatus>(entity => {
            entity.ToTable("OrderStatus");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedOnAdd();
            entity.Property(o => o.OrderStatusName)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(o => o.Description)
                   .HasMaxLength(500);
            entity.Property(o => o.Created)
                   .IsRequired();
            entity.Property(o => o.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(o => o.Modified);
            entity.Property(o => o.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasData(
                new OrderStatus { Id = 1, OrderStatusName = "Ordered", Description = "Order has been placed", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new OrderStatus { Id = 2, OrderStatusName = "ReadyToShip", Description = "Order is ready to ship", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new OrderStatus { Id = 3, OrderStatusName = "Shipped", Description = "Order has been shipped", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new OrderStatus { Id = 4, OrderStatusName = "Closed", Description = "Order has been closed", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" }
            );
        });

        modelBuilder.Entity<BillingMethod>(entity => {
            entity.ToTable("BillingMethod");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).ValueGeneratedOnAdd();
            entity.Property(b => b.BillingMethodName)
                   .IsRequired()
                   .HasMaxLength(100);
            entity.Property(b => b.Description)
                   .HasMaxLength(500);
            entity.Property(b => b.Created)
                   .IsRequired();
            entity.Property(b => b.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(b => b.Modified);
            entity.Property(b => b.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasData(
                new BillingMethod { Id = 1, BillingMethodName = "CreditCard", Description = "Credit card payment", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new BillingMethod { Id = 2, BillingMethodName = "MasterCard", Description = "MasterCard payment", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new BillingMethod { Id = 3, BillingMethodName = "BankCard", Description = "Bank card payment", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" },
                new BillingMethod { Id = 4, BillingMethodName = "Paypal", Description = "PayPal payment", Status = 1, Created = new DateTime(2026, 3, 21, 0, 0, 0, DateTimeKind.Utc), CreatedBy = "System" }
            );
        });

        modelBuilder.Entity<BankCardInfo>(entity => {
            entity.ToTable("BankCardInfo");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).ValueGeneratedOnAdd();
            entity.Property(b => b.CardNo)
                   .IsRequired()
                   .HasMaxLength(500)
                   .HasConversion(_encryptedConverter);
            entity.Property(b => b.Last4Digit)
                   .IsRequired()
                   .HasMaxLength(4);
            entity.Property(b => b.CVC)
                   .IsRequired()
                   .HasMaxLength(500)
                   .HasConversion(_encryptedConverter);
            entity.Property(b => b.ExpiryMonth)
                   .IsRequired();
            entity.Property(b => b.ExpiryYear)
                   .IsRequired();
            entity.Property(b => b.Created)
                   .IsRequired();
            entity.Property(b => b.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(b => b.Modified);
            entity.Property(b => b.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(b => b.BillingMethod)
                   .WithMany(m => m.BankCardInfos)
                   .HasForeignKey(b => b.BillingMethodId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CustomerBillingCardLink>(entity => {
            entity.ToTable("CustomerBillingCardLink");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedOnAdd();
            entity.Property(l => l.Created)
                   .IsRequired();
            entity.Property(l => l.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(l => l.Modified);
            entity.Property(l => l.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(l => l.Customer)
                   .WithMany(c => c.CustomerBillingCardLinks)
                   .HasForeignKey(l => l.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.BankCardInfo)
                   .WithMany(b => b.CustomerBillingCardLinks)
                   .HasForeignKey(l => l.BankCardInfoId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.ShoppingCart>(entity => {
            entity.ToTable("ShoppingCart");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.Created)
                   .IsRequired();
            entity.Property(s => s.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(s => s.Modified);
            entity.Property(s => s.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(s => s.Customer)
                   .WithMany(c => c.ShoppingCarts)
                   .HasForeignKey(s => s.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShoppingCartDetail>(entity => {
            entity.ToTable("ShoppingCartDetail");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedOnAdd();
            entity.Property(d => d.Quantity)
                   .IsRequired();
            entity.Property(d => d.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            entity.Property(d => d.TotalPrice)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            entity.Property(d => d.Created)
                   .IsRequired();
            entity.Property(d => d.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(d => d.Modified);
            entity.Property(d => d.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(d => d.ShoppingCart)
                   .WithMany(s => s.ShoppingCartDetails)
                   .HasForeignKey(d => d.ShoppingCartId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                   .WithMany(p => p.ShoppingCartDetails)
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImportRecord>(entity => {
            entity.ToTable("ProductImportRecord");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.ImportPrice)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            entity.Property(r => r.Quantity)
                   .IsRequired();
            entity.Property(r => r.Comment)
                   .HasMaxLength(1000);
            entity.Property(r => r.Created)
                   .IsRequired();
            entity.Property(r => r.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(r => r.Modified);
            entity.Property(r => r.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(r => r.Product)
                   .WithMany(p => p.ProductImportRecords)
                   .HasForeignKey(r => r.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductInventory>(entity => {
            entity.ToTable("ProductInventory");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.Quantity)
                   .IsRequired();
            entity.Property(i => i.Description)
                   .HasMaxLength(500);
            entity.Property(i => i.Comment)
                   .HasMaxLength(1000);
            entity.Property(i => i.Created)
                   .IsRequired();
            entity.Property(i => i.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(i => i.Modified);
            entity.Property(i => i.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(i => i.Product)
                   .WithMany(p => p.ProductInventories)
                   .HasForeignKey(i => i.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity => {
            entity.ToTable("Order");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedOnAdd();
            entity.Property(o => o.Created)
                   .IsRequired();
            entity.Property(o => o.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(o => o.Modified);
            entity.Property(o => o.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(o => o.Customer)
                   .WithMany(c => c.Orders)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.BankCardInfo)
                   .WithMany()
                   .HasForeignKey(o => o.BankCardId)
                   .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.ShippingAddress)
                   .WithMany()
                   .HasForeignKey(o => o.ShippingAddressId)
                   .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.BillingAddress)
                   .WithMany()
                   .HasForeignKey(o => o.BillingAddressId)
                   .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.OrderStatus)
                   .WithMany(s => s.Orders)
                   .HasForeignKey(o => o.OrderStatusId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderDetail>(entity => {
            entity.ToTable("OrderDetail");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedOnAdd();
            entity.Property(d => d.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            entity.Property(d => d.Quantity)
                   .IsRequired();
            entity.Property(d => d.TotalPrice)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");
            entity.Property(d => d.Comment)
                   .HasMaxLength(1000);
            entity.Property(d => d.Created)
                   .IsRequired();
            entity.Property(d => d.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(d => d.Modified);
            entity.Property(d => d.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(d => d.Order)
                   .WithMany(o => o.OrderDetails)
                   .HasForeignKey(d => d.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                   .WithMany(p => p.OrderDetails)
                   .HasForeignKey(d => d.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShippingServiceProvider>(entity => {
            entity.ToTable("ShippingServiceProvider");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.ProviderName)
                   .IsRequired()
                   .HasMaxLength(200);
            entity.Property(s => s.Comment)
                   .HasMaxLength(1000);
            entity.Property(s => s.Created)
                   .IsRequired();
            entity.Property(s => s.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(s => s.Modified);
            entity.Property(s => s.ModifiedBy)
                   .HasMaxLength(256);
        });

        modelBuilder.Entity<ShippingTracking>(entity => {
            entity.ToTable("ShippingTracking");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.Property(t => t.TrackingNumber)
                   .IsRequired()
                   .HasMaxLength(200);
            entity.Property(t => t.Comment)
                   .HasMaxLength(1000);
            entity.Property(t => t.ShippingDate)
                   .IsRequired();
            entity.Property(t => t.Created)
                   .IsRequired();
            entity.Property(t => t.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(t => t.Modified);
            entity.Property(t => t.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(t => t.ShippingServiceProvider)
                   .WithMany(s => s.ShippingTrackings)
                   .HasForeignKey(t => t.ShippingServiceProviderId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShippingItemDetail>(entity => {
            entity.ToTable("ShippingItemDetail");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.Created)
                   .IsRequired();
            entity.Property(i => i.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(i => i.Modified);
            entity.Property(i => i.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasIndex(i => i.OrderDetailId)
                   .IsUnique();

            entity.HasOne(i => i.OrderDetail)
                   .WithOne(d => d.ShippingItemDetail)
                   .HasForeignKey<ShippingItemDetail>(i => i.OrderDetailId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRole>(entity => {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => ur.Id);
            entity.Property(ur => ur.Id).ValueGeneratedOnAdd();
            entity.Property(ur => ur.Created)
                   .IsRequired();
            entity.Property(ur => ur.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(ur => ur.Modified);
            entity.Property(ur => ur.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserCustomerLink>(entity => {
            entity.ToTable("UserCustomerLink");
            entity.HasKey(uc => uc.Id);
            entity.Property(uc => uc.Id).ValueGeneratedOnAdd();
            entity.Property(uc => uc.Created)
                   .IsRequired();
            entity.Property(uc => uc.CreatedBy)
                   .HasMaxLength(256);
            entity.Property(uc => uc.Modified);
            entity.Property(uc => uc.ModifiedBy)
                   .HasMaxLength(256);

            entity.HasIndex(uc => uc.UserId)
                   .IsUnique();

            entity.HasIndex(uc => uc.CustomerId)
                   .IsUnique();

            entity.HasOne(uc => uc.User)
                   .WithOne(u => u.UserCustomerLink)
                   .HasForeignKey<UserCustomerLink>(uc => uc.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uc => uc.Customer)
                   .WithOne(c => c.UserCustomerLink)
                   .HasForeignKey<UserCustomerLink>(uc => uc.CustomerId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}