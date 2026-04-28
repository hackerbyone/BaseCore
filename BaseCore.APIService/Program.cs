using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;
using BaseCore.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BaseCore API Service",
        Version = "v1",
        Description = "Business Logic Microservice - SQL Server Version"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// SQL Server Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("ConnectedDb")
    ?? "Server=(localdb)\\mssqllocaldb;Database=BaseCoreSales;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Generic Repository Registration
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specific Repository Registration
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();

// Service Registration
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? "YourSecretKeyForAuthenticationShouldBeLongEnough");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Ensure Database is created and apply schema patches
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        // Schema patch: add DiscountPercent to Products if missing
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'DiscountPercent'
            )
            BEGIN
                ALTER TABLE [Products] ADD [DiscountPercent] decimal(5,2) NOT NULL DEFAULT 0;
                UPDATE [Products] SET [DiscountPercent] = 10 WHERE [Id] = 3;
                UPDATE [Products] SET [DiscountPercent] = 5  WHERE [Id] = 7;
                UPDATE [Products] SET [DiscountPercent] = 15 WHERE [Id] = 12;
                UPDATE [Products] SET [DiscountPercent] = 10 WHERE [Id] = 17;
                PRINT 'Schema patch applied: DiscountPercent added to Products';
            END
        ");

        // Schema patch: create Manufacturers table and add ManufacturerId to Products
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Manufacturers]') AND type = 'U')
            BEGIN
                CREATE TABLE [Manufacturers] (
                    [Id]          int          NOT NULL IDENTITY,
                    [Name]        nvarchar(200) NOT NULL,
                    [Description] nvarchar(500) NULL,
                    [Website]     nvarchar(200) NULL,
                    [Phone]       nvarchar(20)  NULL,
                    CONSTRAINT [PK_Manufacturers] PRIMARY KEY ([Id])
                );
                INSERT INTO [Manufacturers] ([Name],[Description],[Website],[Phone]) VALUES
                (N'Thiên Long', N'Thương hiệu bút viết hàng đầu Việt Nam', N'thienlong.com.vn', NULL),
                (N'Pilot',      N'Thương hiệu bút Nhật Bản nổi tiếng',     NULL, NULL),
                (N'Staedtler',  N'Thương hiệu dụng cụ viết của Đức',        NULL, NULL),
                (N'Pentel',     N'Thương hiệu văn phòng phẩm Nhật Bản',     NULL, NULL),
                (N'Kangaro',    N'Thương hiệu dụng cụ văn phòng Ấn Độ',     NULL, NULL),
                (N'Casio',      N'Thương hiệu điện tử Nhật Bản',             NULL, NULL),
                (N'3M / Post-it',N'Thương hiệu văn phòng phẩm Mỹ',          NULL, NULL),
                (N'Nội địa',    N'Sản phẩm sản xuất trong nước',             NULL, NULL);
                PRINT 'Schema patch applied: Manufacturers table created';
            END
        ");

        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ManufacturerId'
            )
            BEGIN
                ALTER TABLE [Products] ADD [ManufacturerId] int NULL;
                IF OBJECT_ID(N'[Manufacturers]') IS NOT NULL
                BEGIN
                    ALTER TABLE [Products]
                        ADD CONSTRAINT [FK_Products_Manufacturers_ManufacturerId]
                        FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturers]([Id])
                        ON DELETE SET NULL;
                END
                PRINT 'Schema patch applied: ManufacturerId added to Products';
            END
        ");

        Console.WriteLine("Database ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARNING] Database init failed: {ex.Message}");
        Console.WriteLine("Service will start, but API calls may fail until database is accessible.");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Serve uploaded product images from wwwroot/uploads/
var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
Directory.CreateDirectory(uploadsDir);
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("BaseCore API Service (SQL Server) running on port 5001");
app.Run();
