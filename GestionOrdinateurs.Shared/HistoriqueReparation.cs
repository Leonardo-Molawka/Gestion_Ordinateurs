namespace GestionOrdinateurs.Shared;

public record HistoriqueReparation(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention);
