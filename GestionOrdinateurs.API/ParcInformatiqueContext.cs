using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class ParcInformatiqueContext : DbContext
{
    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : base(options)
    {
    }

    public DbSet<Ordinateur> Ordinateurs { get; set; }
    public DbSet<Reparation> Reparations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Login)
            .IsUnique();

        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(ordinateur => ordinateur.Caracteristiques, configuration =>
            {
                configuration.Property(caracteristiques => caracteristiques.RamGo).HasColumnName("RamGo");
                configuration.Property(caracteristiques => caracteristiques.TypeOrdi).HasColumnName("TypeOrdi");
                configuration.Property(caracteristiques => caracteristiques.StockageGo).HasColumnName("StockageGo");
            });

        modelBuilder.Entity<Ordinateur>()
            .HasMany(ordinateur => ordinateur.Reparations)
            .WithOne(reparation => reparation.UnOrdinateur)
            .HasForeignKey(reparation => reparation.OrdinateurId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Stock)
            .HasDefaultValue(0);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Prix)
            .HasColumnType("TEXT");

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Vendu)
            .HasDefaultValue(false);
    }

    protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
        }
    }
}