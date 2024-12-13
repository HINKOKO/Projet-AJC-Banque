using LibAJCBanque;
using Microsoft.EntityFrameworkCore;

namespace ApiAJCBanque.Datas
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Adresse> Adresses { get; set; }
        public DbSet<Carte> Cartes { get; set; }
        public DbSet<Compte> Comptes { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientPar> ClientsPar { get; set; }
        public DbSet<ClientPro> ClientsPro { get; set; }
        public DbSet<Operation> Operations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            // API Fluent
            //TPH Table Per Hierarchy
            modelBuilder.Entity<Client>()
                  .HasDiscriminator<string>("Type")
                  .HasValue<ClientPar>("Particulier")
                  .HasValue<ClientPro>("Professionnel"); ;         

            modelBuilder.Entity<Client>().ToTable("Client");
            modelBuilder.Entity<ClientPar>().ToTable("Client");
            modelBuilder.Entity<ClientPro>().ToTable("Client");

            modelBuilder.Entity<Carte>().ToTable("Carte");
            modelBuilder.Entity<Adresse>().ToTable("Adresse");

            modelBuilder.Entity<Compte>().ToTable("Compte");
            modelBuilder.Entity<Compte>().Property("Solde").HasColumnType("decimal(18,2)");
            
            modelBuilder.Entity<Operation>().ToTable("Operation");
            modelBuilder.Entity<Operation>().Property("Montant").HasColumnType("decimal(18,2)");
        }
    }
}

