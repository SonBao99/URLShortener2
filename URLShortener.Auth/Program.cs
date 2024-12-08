using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using URLShortener.Auth.Data;
using URLShortener.Auth.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins(
                "http://localhost:60443",
                "https://localhost:60443",
                "http://localhost:5000",
                "https://localhost:5000",
                "http://localhost:7214",
                "https://localhost:7214"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
        builder.WithOrigins("http://localhost:60443")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials());
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
    options.DefaultAuthenticateScheme = "ApplicationCookie";
    options.DefaultSignInScheme = "ExternalCookie";
    options.DefaultChallengeScheme = "Google";
})
.AddCookie("ApplicationCookie")
.AddCookie("ExternalCookie", options =>
{
    options.Cookie.Name = ".AspNetCore.External";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/api/auth/google-callback";
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    
    options.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = context =>
        {
            context.RedirectUri = context.RedirectUri;
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            context.Response.Redirect($"{builder.Configuration["FrontendUrl"]}/auth-callback?error={Uri.EscapeDataString(context.Failure?.Message ?? "Authentication failed")}");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// Add JWT Bearer configuration separately
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });

// Add services to the container
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
