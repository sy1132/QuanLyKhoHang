using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLKhoHang.Entities;
namespace QLKhoHang.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet <Category> Categories { get; set; }
        public DbSet <Import> Imports { get; set; }
        public DbSet <Products> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet <Warehouse> Warehouse { get; set; }
        public DbSet <ImportDetail> ImportDetails { get; set; }
    }
}