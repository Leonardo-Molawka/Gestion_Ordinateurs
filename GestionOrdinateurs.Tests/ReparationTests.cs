using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class ReparationTests
{
    [Fact]
    public void Creation_InitialiseLesDonneesDeReparation()
    {
        DateTime dateDebut = new(2026, 10, 14, 10, 0, 0);
        DateTime dateIntervention = new(2026, 10, 14, 11, 0, 0);

        Reparation reparation = new()
        {
            OrdinateurId = 1,
            TypeReparation = "Remplacement disque",
            Technicien = "Alice",
            DateDebut = dateDebut,
            DateIntervention = dateIntervention,
            Statut = "En cours"
        };

        Assert.Equal(1, reparation.OrdinateurId);
        Assert.Equal("Remplacement disque", reparation.TypeReparation);
        Assert.Equal("Alice", reparation.Technicien);
        Assert.Equal(dateDebut, reparation.DateDebut);
        Assert.Equal(dateIntervention, reparation.DateIntervention);
        Assert.Equal("En cours", reparation.Statut);
    }
}

public class OrdinateurServiceTests
{
    [Fact]
    public async Task OperationsCrud_DeclenchentLesEvenementsEtMettentLesDonneesAJour()
    {
        using ParcInformatiqueContext context = CreerContexte();
        OrdinateurService service = CreerService(context);
        OrdinateurAjouteEventArgs? ajout = null;
        OrdinateurMisAJourEventArgs? modification = null;
        OrdinateurSupprimeEventArgs? suppression = null;
        service.OrdinateurAjoute += (_, args) => ajout = args;
        service.OrdinateurMisAJour += (_, args) => modification = args;
        service.OrdinateurSupprime += (_, args) => suppression = args;

        Ordinateur ordinateur = CreerOrdinateur("Avant");
        await service.AjouterOrdinateurAsync(ordinateur, CancellationToken.None);
        Assert.Same(ordinateur, Assert.IsType<OrdinateurAjouteEventArgs>(ajout).Ordinateur);

        Ordinateur ordinateurModifie = new(
            "Après",
            EtatOrdinateur.EnService,
            5,
            799.99m,
            32,
            1024,
            TypeOrdinateur.Server);
        Assert.True(await service.MiseAJourOrdinateurAsync(
            ordinateur.Id,
            ordinateurModifie,
            CancellationToken.None));

        Ordinateur modifie = Assert.IsType<OrdinateurMisAJourEventArgs>(modification).Ordinateur;
        Assert.Equal("Après", modifie.Nom);
        Assert.Equal(5, modifie.Stock);
        Assert.Equal(799.99m, modifie.Prix);
        Assert.Equal(32, modifie.Caracteristiques.RamGo);
        Assert.Equal(TypeOrdinateur.Server, modifie.Caracteristiques.TypeOrdi);

        Assert.True(await service.SupprimerOrdinateurAsync(ordinateur.Id, CancellationToken.None));
        Assert.Same(modifie, Assert.IsType<OrdinateurSupprimeEventArgs>(suppression).Ordinateur);
    }

    [Fact]
    public async Task SimulerReparations_CreeCinqReparationsAssocieesEtHistorisees()
    {
        string databaseName = Guid.NewGuid().ToString();
        using ServiceProvider provider = new ServiceCollection()
            .AddDbContextFactory<ParcInformatiqueContext>(options =>
                options.UseInMemoryDatabase(databaseName))
            .AddSingleton<JournalService>()
            .AddScoped<ReparationService>()
            .BuildServiceProvider();

        IDbContextFactory<ParcInformatiqueContext> contextFactory =
            provider.GetRequiredService<IDbContextFactory<ParcInformatiqueContext>>();
        Ordinateur[] ordinateurs = Enumerable.Range(1, 5)
            .Select(index => new Ordinateur(
                $"Test {index}",
                EtatOrdinateur.EnService,
                1,
                100m,
                8,
                256,
                TypeOrdinateur.Portable)
            {
                Id = index
            })
            .ToArray();

        await using (ParcInformatiqueContext context = await contextFactory.CreateDbContextAsync())
        {
            context.Ordinateurs.AddRange(ordinateurs);
            await context.SaveChangesAsync();
        }

        ReparationService service = provider.GetRequiredService<ReparationService>();
        await Task.WhenAll(ordinateurs.Select(ordinateur =>
            service.SimulerReparationAsync(ordinateur, CancellationToken.None)));

        List<Reparation> reparations =
            await service.ListerReparationsAsync(CancellationToken.None);
        List<HistoriqueReparationRecord> historique =
            await service.ObtenirHistoriqueAsync(CancellationToken.None);

        Assert.Equal(5, reparations.Count);
        Assert.All(reparations, reparation =>
        {
            Assert.Equal("Terminée", reparation.Statut);
            Assert.NotNull(reparation.UnOrdinateur);
            Assert.True(reparation.DateIntervention >= reparation.DateDebut);
        });
        Assert.Equal(5, historique.Count);
        Assert.Equal(5, historique.Select(record => record.OrdinateurId).Distinct().Count());
    }

    private static OrdinateurService CreerService(ParcInformatiqueContext context)
    {
        return new OrdinateurService(context, new JournalService());
    }

    private static ParcInformatiqueContext CreerContexte()
    {
        DbContextOptions<ParcInformatiqueContext> options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        ParcInformatiqueContext context = new(options);
        context.Ordinateurs.AddRange(
            new Ordinateur(
                "Test 1",
                EtatOrdinateur.EnService,
                0,
                100m,
                8,
                256,
                TypeOrdinateur.Portable)
            {
                Id = 1
            },
            new Ordinateur(
                "Test 2",
                EtatOrdinateur.EnService,
                3,
                250m,
                16,
                512,
                TypeOrdinateur.Fixe)
            {
                Id = 2
            });
        context.SaveChanges();
        return context;
    }

    private static Ordinateur CreerOrdinateur(string nom)
    {
        return new Ordinateur(
            nom,
            EtatOrdinateur.EnService,
            1,
            100m,
            16,
            512,
            TypeOrdinateur.Portable);
    }
}
