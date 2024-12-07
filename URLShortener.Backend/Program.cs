using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Policy to allow React app to communicate with the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
        builder.WithOrigins("http://localhost:60443")  // React development server
               .AllowAnyMethod()
               .AllowAnyHeader());
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
