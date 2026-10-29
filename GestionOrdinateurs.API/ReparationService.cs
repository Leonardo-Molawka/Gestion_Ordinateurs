using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention);

public class ReparationService
{
    private readonly IDbContextFactory<ParcInformatiqueContext> contextFactory;
    private readonly JournalService journalService;

    public ReparationService(
        IDbContextFactory<ParcInformatiqueContext> contextFactory,
        JournalService journalService)
    {
        this.contextFactory = contextFactory;
        this.journalService = journalService;
    }

    public async Task SimulerReparationAsync(
        Ordinateur ordinateur,
        CancellationToken cancellationToken)
    {
        await using ParcInformatiqueContext context =
            await contextFactory.CreateDbContextAsync(cancellationToken);

        Reparation reparation = new()
        {
            OrdinateurId = ordinateur.Id,
            TypeReparation = "Test",
            Technicien = "Automatique",
            DateDebut = DateTime.Now,
            DateIntervention = DateTime.Now,
            Statut = "En cours"
        };

        context.Reparations.Add(reparation);
        await Task.Delay(2000, cancellationToken);

        reparation.Statut = "Terminée";
        reparation.DateIntervention = DateTime.Now;
        await context.SaveChangesAsync(cancellationToken);

        journalService.Ecrire($"{ordinateur.Nom} réparé");
        Console.WriteLine($"{ordinateur.Nom} réparé");
    }

    public async Task<List<Reparation>> ListerReparationsAsync(
        CancellationToken cancellationToken)
    {
        await using ParcInformatiqueContext context =
            await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Reparations
            .AsNoTracking()
            .Include(reparation => reparation.UnOrdinateur)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync(
        CancellationToken cancellationToken)
    {
        await using ParcInformatiqueContext context =
            await contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Reparations
            .AsNoTracking()
            .Include(reparation => reparation.UnOrdinateur)
            .Select(reparation => new HistoriqueReparationRecord(
                reparation.OrdinateurId,
                reparation.UnOrdinateur.Nom,
                reparation.Technicien,
                reparation.TypeReparation,
                reparation.DateIntervention))
            .ToListAsync(cancellationToken);
    }
}
