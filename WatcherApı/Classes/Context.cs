
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WatcherApi.Classes
{
    public class Context :IdentityDbContext<User,Role,int>
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=192.168.10.18;Database=VirtualDB;User Id=sa;Password=1q2w3e4r+!;TrustServerCertificate=True");
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<MachineInfo> Machines { get; set; }

    }
}
