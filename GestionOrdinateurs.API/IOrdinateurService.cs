using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IOrdinateurService
{
    event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    Task AjouterOrdinateurAsync(Ordinateur ordinateur, CancellationToken cancellationToken);
    Task<bool> SupprimerOrdinateurAsync(int id, CancellationToken cancellationToken);
    Task<bool> MiseAJourOrdinateurAsync(int id, Ordinateur ordinateur, CancellationToken cancellationToken);
    Task<List<Ordinateur>> ListerOrdinateursAsync(CancellationToken cancellationToken);
    Task<Ordinateur?> RenvoyerOrdinateurParIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Ordinateur>> ListerOrdinateursDisponiblesAsync(CancellationToken cancellationToken);
    Task<List<Ordinateur>> ListerParPrixDecroissantAsync(CancellationToken cancellationToken);
    string Diagnostiquer(Ordinateur ordinateur);
    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();
}