using Domain.Models;
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
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Job> Jobs { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder){

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Job>(ops => 
                {   
                    ops.HasKey(ur => new {ur.CustomerId, ur.UserId}) ;
                }
            );

            // n p/ n
            
        }


    }
}