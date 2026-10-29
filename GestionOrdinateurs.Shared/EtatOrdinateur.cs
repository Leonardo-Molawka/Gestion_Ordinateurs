namespace GestionOrdinateurs.Shared;

public enum EtatOrdinateur
{
    EnService = 3,
    EnPanne = 4,
    EnMaintenance = 5,
    HorsService = 6,
    HS = HorsService,
    Lent = 7,
    Reparable = 8
}