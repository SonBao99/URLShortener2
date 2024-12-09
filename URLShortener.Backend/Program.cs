using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UrlShortener.Messaging.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// CORS Policy to allow React app to communicate with the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
        builder.WithOrigins("http://localhost:60443")  // React development server
               .AllowAnyMethod()
               .AllowAnyHeader());
});

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

// Add RabbitMQ Configuration
builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var isDevelopment = builder.Environment.IsDevelopment();
    var host = isDevelopment ? "localhost" : "rabbitmq";
    var connectionString = $"amqp://guest:guest@{host}:5672";
    return new RabbitMQService(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Use CORS for the React app
app.UseCors("AllowReactApp");

// Enable routing and endpoint mapping
app.UseRouting();

// Enable authorization (if needed, depending on your app's requirements)
app.UseAuthorization();

// Map API controllers
app.MapControllers();

app.Run();
