using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure the port before building the app
builder.WebHost.UseUrls("https://localhost:5000");

// Register Ocelot and other services
builder.Services.AddOcelot();

// Configure authentication with JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // The Authority is the Auth service (where the token is issued)
        options.Authority = "https://localhost:7214"; // URL of your Auth service
        options.Audience = "https://localhost:5000"; // Expected audience for the JWT token
        options.RequireHttpsMetadata = true; // Set to true for production
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://localhost:7214", // Set this to match the issuer in your Auth service
            ValidAudience = "https://localhost:5000", // Set this to the expected audience (ensure it matches the Auth service)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Sveřepí šakali zavile vyli na bílý měsíc")) // Use the same key as in Auth service
        };
    });

var app = builder.Build();

// Enable authentication middleware
app.UseAuthentication(); // This is necessary to validate the JWT token
app.UseAuthorization();  // Ensure the app checks for authorization after authentication

// Enable Ocelot middleware
app.UseOcelot();

app.Run();
