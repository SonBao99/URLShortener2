using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UrlShortener.Backend.Models;

namespace UrlShortener.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<ShortUrl> ShortUrls { get; set; }
}
