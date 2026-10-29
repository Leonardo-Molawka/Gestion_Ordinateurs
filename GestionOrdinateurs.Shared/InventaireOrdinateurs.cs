namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = [];

    public Ordinateur? this[int index]
    {
        get => Ordinateurs.FirstOrDefault(ordinateur => ordinateur.Id == index);
    }

    public Ordinateur? this[string reference]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return null;
            }

            return Ordinateurs.FirstOrDefault(ordinateur =>
                string.Equals(ordinateur.Nom, reference, StringComparison.OrdinalIgnoreCase) ||
                ordinateur.Id.ToString() == reference);
        }
    }
}
