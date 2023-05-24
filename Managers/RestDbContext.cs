using Microsoft.EntityFrameworkCore;
using Redis_caching.Models;

namespace Redis_caching.Managers;

public class RestDbContext: DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public RestDbContext(DbContextOptions<RestDbContext> options) : base(options)
    {
    }
}