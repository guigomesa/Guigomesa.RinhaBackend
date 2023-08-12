using Microsoft.EntityFrameworkCore;
using Models;

namespace Data {
    public class RinhaContext : DbContext {
        public RinhaContext(DbContextOptions<RinhaContext> options) : base(options) { }

        public DbSet<Pessoa> Pessoas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("RinhaContext"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pessoa>().ToTable("Pessoas")
            .HasIndex("Apelido").IsUnique();
        }
    }
}