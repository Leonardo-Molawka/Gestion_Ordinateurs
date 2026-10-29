using System.ComponentModel;
using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;

public class Ordinateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public EtatOrdinateur Etat { get; set; }

    [DefaultValue(0)]
    public int Stock { get; set; }

    [DefaultValue(0)]
    public decimal Prix { get; set; }

    public bool Vendu { get; set; }

    public Caracteristiques Caracteristiques { get; set; } = new();

    [JsonIgnore]
    public List<Reparation> Reparations { get; set; } = new();

    public Ordinateur()
    {
        Nom = string.Empty;
        Caracteristiques = new Caracteristiques();
    }

    public Ordinateur(string nom, EtatOrdinateur etat, int stock = 0, decimal prix = 0m, int ramGo = 0, int stockageGo = 0, TypeOrdinateur typeOrdi = TypeOrdinateur.Portable)
    {
        Nom = nom;
        Etat = etat;
        Stock = stock;
        Prix = prix;
        Caracteristiques = new Caracteristiques(ramGo, stockageGo, typeOrdi);
    }
}