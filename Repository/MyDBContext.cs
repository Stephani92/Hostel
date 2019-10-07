using Identity.Reposi;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity
{
    public class MyDBContext : IdentityDbContext<User>
    {
        public MyDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Role> Role { get; set; }
        public DbSet<User> User { get; set; }


    }
}