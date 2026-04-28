using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;
using BaseCore.Services.Authen;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BaseCore Auth Service API",
        Version = "v1",
        Description = "Authentication Microservice - SQL Server Version"
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
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

// SQL Server Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                      ?? builder.Configuration.GetConnectionString("ConnectedDb")
                      ?? "Server=(localdb)\\mssqllocaldb;Database=BaseCoreSales;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// DI for Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();

// DI for Services
builder.Services.AddScoped<IUserService, UserService>();

// JWT Authentication Key
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Ensure Database is created and seed admin user
using (var scope = app.Services.CreateScope())
{
    AppDbContext db;
    try
    {
        db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        Console.WriteLine("Database ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WARNING] Database init failed: {ex.Message}");
        Console.WriteLine("Service will start, but API calls may fail until database is accessible.");
        db = null;
    }

    if (db != null && !db.Users.Any(u => u.UserName == "admin"))
    {
        byte[] salt;
        var hashedPassword = BaseCore.Common.TokenHelper.HashPassword("admin123", out salt);
        db.Users.Add(new BaseCore.Entities.User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "admin",
            Name = "Administrator",
            Password = hashedPassword,
            Salt = salt,
            Email = "admin@basecore.com",
            Contact = "",
            Phone = "",
            Position = "",
            Image = "",
            IsActive = true,
            UserType = 1,
            Created = DateTime.Now
        });
        db.SaveChanges();
        Console.WriteLine("==> Admin user created: username=admin / password=admin123");
    }
}

Console.WriteLine("BaseCore Auth Service (SQL Server) running on port 5002");
app.Run();
