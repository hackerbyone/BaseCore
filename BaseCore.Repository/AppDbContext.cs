using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;

namespace BaseCore.Repository
{
    /// <summary>
    /// Entity Framework Core DbContext for SQL Server
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Authentication & Authorization
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<ModuleFunction> ModuleFunctions { get; set; }
        public DbSet<RoleModuleFunction> RoleModuleFunctions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserModule> UserModules { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Setting> Settings { get; set; }

        // Core Business Entities
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Logging
        public DbSet<LogAction> LogActions { get; set; }
        public DbSet<LogError> LogErrors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.HasIndex(e => e.UserName).IsUnique();
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure Manufacturer entity
            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.DiscountPercent).HasPrecision(5, 2).HasDefaultValue(0);

                // Relationship with Category
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relationship with Manufacturer (nullable)
                entity.HasOne(e => e.Manufacturer)
                      .WithMany()
                      .HasForeignKey(e => e.ManufacturerId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            });

            // Configure OrderDetail entity
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                // Relationships
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure LogAction entity
            modelBuilder.Entity<LogAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(200);
            });

            // Configure LogError entity
            modelBuilder.Entity<LogError>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(450);
                entity.Property(e => e.Message).IsRequired();
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Manufacturers
            modelBuilder.Entity<Manufacturer>().HasData(
                new Manufacturer { Id = 1, Name = "Thiên Long", Description = "Thương hiệu bút viết hàng đầu Việt Nam", Website = "thienlong.com.vn" },
                new Manufacturer { Id = 2, Name = "Pilot", Description = "Thương hiệu bút Nhật Bản nổi tiếng" },
                new Manufacturer { Id = 3, Name = "Staedtler", Description = "Thương hiệu dụng cụ viết của Đức" },
                new Manufacturer { Id = 4, Name = "Pentel", Description = "Thương hiệu văn phòng phẩm Nhật Bản" },
                new Manufacturer { Id = 5, Name = "Kangaro", Description = "Thương hiệu dụng cụ văn phòng Ấn Độ" },
                new Manufacturer { Id = 6, Name = "Casio", Description = "Thương hiệu điện tử Nhật Bản" },
                new Manufacturer { Id = 7, Name = "3M / Post-it", Description = "Thương hiệu văn phòng phẩm Mỹ" },
                new Manufacturer { Id = 8, Name = "Nội địa", Description = "Sản phẩm sản xuất trong nước" }
            );

            // Seed Categories - Văn phòng phẩm
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Văn phòng phẩm", Description = "Đồ dùng văn phòng tổng hợp" },
                new Category { Id = 2, Name = "Bút viết", Description = "Các loại bút bi, mực, dạ" },
                new Category { Id = 3, Name = "Giấy in & Vở", Description = "Giấy in, vở viết, sổ tay" },
                new Category { Id = 4, Name = "Dụng cụ bàn làm việc", Description = "Kéo, thước, dập ghim, v.v." },
                new Category { Id = 5, Name = "Kẹp & Ghim", Description = "Kẹp giấy, ghim, dây buộc" }
            );

            // Seed Products - 20 sản phẩm văn phòng phẩm
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1,  Name = "Bút bi Thiên Long TL-027",      Price = 5000,   Stock = 500, CategoryId = 2, Description = "Bút bi mực xanh, ngòi 0.7mm, viết mượt mà",                         ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 2,  Name = "Bút bi Bến Nghé xanh",           Price = 3500,   Stock = 400, CategoryId = 2, Description = "Bút bi giá rẻ, thông dụng văn phòng",                               ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 3,  Name = "Bút mực Pilot V5",               Price = 22000,  Stock = 150, CategoryId = 2, Description = "Bút mực nước màu xanh, ngòi kim 0.5mm",                             ImageUrl = "", DiscountPercent = 10 },
                new Product { Id = 4,  Name = "Bút dạ quang vàng",              Price = 12000,  Stock = 200, CategoryId = 2, Description = "Bút highlight màu vàng, dạ quang",                                  ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 5,  Name = "Bút xóa nước trắng",             Price = 8000,   Stock = 120, CategoryId = 2, Description = "Bút xóa nhanh khô, không lem",                                      ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 6,  Name = "Vở ô ly 200 trang",              Price = 18000,  Stock = 300, CategoryId = 3, Description = "Vở học sinh 200 trang, kẻ ô ly 5mm",                               ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 7,  Name = "Tập giấy A4 Bãi Bằng 70gsm",    Price = 55000,  Stock = 100, CategoryId = 3, Description = "500 tờ giấy A4, trắng sáng, 70gsm",                                 ImageUrl = "", DiscountPercent = 5  },
                new Product { Id = 8,  Name = "Giấy ghi chú Post-it 76x76mm",   Price = 25000,  Stock = 80,  CategoryId = 3, Description = "100 tờ/xấp, nhiều màu sắc",                                        ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 9,  Name = "Sổ tay bìa cứng A5",             Price = 45000,  Stock = 60,  CategoryId = 3, Description = "Sổ tay 96 trang, kẻ dòng",                                         ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 10, Name = "Kéo văn phòng 21cm",             Price = 32000,  Stock = 70,  CategoryId = 4, Description = "Kéo inox sắc bén, cán nhựa chống trượt",                           ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 11, Name = "Thước kẻ nhựa 30cm",             Price = 10000,  Stock = 150, CategoryId = 4, Description = "Thước nhựa trong suốt, chia cm và mm",                             ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 12, Name = "Dập ghim Kangaro 26/6",           Price = 65000,  Stock = 40,  CategoryId = 4, Description = "Dập ghim văn phòng, đóng được 20 tờ",                              ImageUrl = "", DiscountPercent = 15 },
                new Product { Id = 13, Name = "Ghim bấm 26/6 hộp 1000 cái",     Price = 15000,  Stock = 200, CategoryId = 5, Description = "Ghim thép không gỉ, phù hợp dập ghim thông thường",               ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 14, Name = "Kẹp giấy bướm số 2 hộp",         Price = 12000,  Stock = 180, CategoryId = 5, Description = "Hộp 12 cái kẹp bướm màu đen, kẹp chắc",                           ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 15, Name = "Kẹp giấy kim hộp 100 cái",       Price = 8000,   Stock = 250, CategoryId = 5, Description = "Kẹp giấy hình chữ L kim loại, tiện dụng",                         ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 16, Name = "Băng dính trong 48mm",            Price = 18000,  Stock = 90,  CategoryId = 4, Description = "Cuộn băng dính trong suốt, không vàng ố",                         ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 17, Name = "Hộp đựng bút để bàn",            Price = 55000,  Stock = 35,  CategoryId = 4, Description = "Hộp nhựa đựng bút đa năng, 3 ngăn",                               ImageUrl = "", DiscountPercent = 10 },
                new Product { Id = 18, Name = "Máy tính Casio FX-570VN",         Price = 250000, Stock = 25,  CategoryId = 4, Description = "Máy tính khoa học 417 hàm, dùng cho học sinh - sinh viên",         ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 19, Name = "Bút chì 2B Staedtler",            Price = 5000,   Stock = 300, CategoryId = 2, Description = "Bút chì gỗ ruột mềm 2B, vẽ và viết",                              ImageUrl = "", DiscountPercent = 0  },
                new Product { Id = 20, Name = "Tẩy nhựa Pentel Hi-Polymer",      Price = 8000,   Stock = 200, CategoryId = 4, Description = "Tẩy sạch, không gây xước giấy",                                   ImageUrl = "", DiscountPercent = 0  }
            );
        }
    }
}

