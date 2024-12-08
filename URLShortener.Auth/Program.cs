using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using URLShortener.Auth.Data;
using URLShortener.Auth.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS to allow Ocelot Gateway to connect
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOcelot", policy =>
    {
        policy.WithOrigins("https://localhost:5000") // The Ocelot API Gateway URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add DbContext with SQL Server and Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add authentication and authorization services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://localhost:7214",  // Make sure this matches the Ocelot Gateway configuration
        ValidAudience = "https://localhost:5000", // Match with Gateway configuration
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Sveřepí šakali zavile vyli na bílý měsíc"))
    };
});


// Add services to the container
builder.Services.AddControllers();

var app = builder.Build();

// Enable CORS to allow Ocelot API Gateway to make requests
app.UseCors("AllowOcelot");

// Use Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers (ensure your routes are correctly configured)
app.MapControllers();

app.Run();
