using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WatcherApi.Classes
{
    public class DataContext:DbContext
    {
        public DbSet<Address> Address { get; set; }
        public DbSet<Sendung> Sendung { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Uzak MySQL veritabanına bağlantı
            optionsBuilder.UseMySql("Server=217.160.27.147;port=3306;database=taskinAmendWeb;user=root;password=8DCMu9_r8v;",
                new MySqlServerVersion(new Version(8, 0, 26))); 
        }

    }

    public class Address
    {
        [Key]
        public int Id { get; set; }
        
        public string CITY { get; set; }
        public string COUNTRY { get; set; }
        public string KDNR { get; set; }
        public string NAME1 { get; set; }
        public string STREET { get; set; }
        public string ZIPCODE { get; set; }
    }

    public class Sendung
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Refnr { get; set; }
    }

    public class LoadInput
    {
        public DateTime Erstellungsdatum { get; set; }
        public int Referenz { get; set; }
        public string Ersteller { get; set; }
    }
}

