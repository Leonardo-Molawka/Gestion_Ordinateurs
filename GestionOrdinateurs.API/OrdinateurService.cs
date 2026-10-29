namespace GestionOrdinateurs.API;

using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

public class OrdinateurService : IOrdinateurService
{
    private readonly ParcInformatiqueContext context;
    private readonly JournalService journalService;

    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    public OrdinateurService(
        ParcInformatiqueContext context,
        JournalService journalService)
    {
        this.context = context;
        this.journalService = journalService;
    }

    public async Task AjouterOrdinateurAsync(
        Ordinateur ordinateur,
        CancellationToken cancellationToken)
    {
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync(cancellationToken);
        journalService.EcrireOrdinateur($"Ordinateur {ordinateur.Nom} ajouté");
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
    }

    public async Task<bool> SupprimerOrdinateurAsync(
        int id,
        CancellationToken cancellationToken)
    {
        Ordinateur? ordinateur = await context.Ordinateurs
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (ordinateur == null)
        {
            return false;
        }

        context.Ordinateurs.Remove(ordinateur);
        await context.SaveChangesAsync(cancellationToken);
        journalService.EcrireOrdinateur($"Ordinateur {ordinateur.Nom} supprimé");
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        return true;
    }

    public async Task<bool> MiseAJourOrdinateurAsync(
        int id,
        Ordinateur ordinateur,
        CancellationToken cancellationToken)
    {
        Ordinateur? ordinateurExistant = await context.Ordinateurs
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (ordinateurExistant == null)
        {
            return false;
        }

        ordinateurExistant.Nom = ordinateur.Nom;
        ordinateurExistant.Etat = ordinateur.Etat;
        ordinateurExistant.Stock = ordinateur.Stock;
        ordinateurExistant.Prix = ordinateur.Prix;
        ordinateurExistant.Vendu = ordinateur.Vendu;

        if (ordinateur.Caracteristiques != null)
        {
            ordinateurExistant.Caracteristiques = new Caracteristiques(
                ordinateur.Caracteristiques.RamGo,
                ordinateur.Caracteristiques.StockageGo,
                ordinateur.Caracteristiques.TypeOrdi);
        }

        await context.SaveChangesAsync(cancellationToken);
        journalService.EcrireOrdinateur($"Ordinateur {ordinateurExistant.Nom} modifié");
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(ordinateurExistant));
        return true;
    }

    public Task<List<Ordinateur>> ListerOrdinateursAsync(CancellationToken cancellationToken)
    {
        return context.Ordinateurs
            .Include(ordinateur => ordinateur.Caracteristiques)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Ordinateur?> RenvoyerOrdinateurParIdAsync(int id, CancellationToken cancellationToken)
    {
        return context.Ordinateurs
            .Include(ordinateur => ordinateur.Caracteristiques)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<List<Ordinateur>> ListerOrdinateursDisponiblesAsync(
        CancellationToken cancellationToken)
    {
        return context.Ordinateurs
            .Include(ordinateur => ordinateur.Caracteristiques)
            .AsNoTracking()
            .Where(ordinateur => ordinateur.Stock > 0)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Ordinateur>> ListerParPrixDecroissantAsync(
        CancellationToken cancellationToken)
    {
        return context.Ordinateurs
            .Include(ordinateur => ordinateur.Caracteristiques)
            .AsNoTracking()
            .OrderByDescending(ordinateur => ordinateur.Prix)
            .ToListAsync(cancellationToken);
    }

    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            { Etat: EtatOrdinateur.HS } => "Remplacement",
            { Etat: EtatOrdinateur.Lent, Caracteristiques: { RamGo: < 8 } } => "Ajouter RAM",
            _ => "Aucune action"
        };
    }

    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques()
    {
        List<Ordinateur> ordinateurs = context.Ordinateurs.AsNoTracking().ToList();
        int total = ordinateurs.Count;
        int disponibles = ordinateurs.Count(ordinateur => ordinateur.Stock > 0);
        decimal valeurStock = ordinateurs.Sum(ordinateur => ordinateur.Stock * ordinateur.Prix);
        int nbReparations = context.Reparations.Count();

        return (total, disponibles, valeurStock, nbReparations);
    }
}