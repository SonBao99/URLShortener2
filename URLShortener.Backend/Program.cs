using Microsoft.EntityFrameworkCore;
using UrlShortener.Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    // Route for POST /shorten
    endpoints.MapControllerRoute(
        name: "shorten",
        pattern: "shorten",
        defaults: new { controller = "Url", action = "ShortenUrl" });

    // Route for GET /{shortCode}
    endpoints.MapControllerRoute(
        name: "redirect",
        pattern: "{shortCode}",
        defaults: new { controller = "Url", action = "RedirectToOriginalUrl" });
});

app.Run();
