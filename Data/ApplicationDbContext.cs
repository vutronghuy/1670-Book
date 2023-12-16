using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _1670_Book.Models;

namespace _1670_Book.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<_1670_Book.Models.Book> Book { get; set; } = default!;
        public DbSet<_1670_Book.Models.Category> Category { get; set; } = default!;
        public DbSet<_1670_Book.Models.Order> Order { get; set; } = default!;
    }
}