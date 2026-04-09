using Microsoft.EntityFrameworkCore;
using User.Entities;

namespace User.Data
{

    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {

        }
        public DbSet<EntitieUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EntitieUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<EntitieUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();
        }
    }

}
