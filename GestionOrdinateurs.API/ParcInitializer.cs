namespace GestionOrdinateurs.Shared;

public static class ParcInitializer
{
    public static List<Ordinateur> InitialiserParc()
    {
        return
        [
            new Ordinateur
            {
                Id = 1,
                Nom = "Ordinateur Fixe 1",
                Etat = EtatOrdinateur.EnService,
                Stock = 2,
                Prix = 1200m,
                Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Fixe)
            },
            new Ordinateur
            {
                Id = 2,
                Nom = "Ordinateur Portable 1",
                Etat = EtatOrdinateur.EnService,
                Stock = 15,
                Prix = 950m,
                Caracteristiques = new Caracteristiques(8, 256, TypeOrdinateur.Portable)
            },
            new Ordinateur
            {
                Id = 3,
                Nom = "Ordinateur Fixe 2",
                Etat = EtatOrdinateur.EnService,
                Stock = 1,
                Prix = 1500m,
                Caracteristiques = new Caracteristiques(32, 1024, TypeOrdinateur.Fixe)
            },
            new Ordinateur
            {
                Id = 4,
                Nom = "Ordinateur Portable 2",
                Etat = EtatOrdinateur.EnService,
                Stock = 14,
                Prix = 1100m,
                Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Portable)
            },
            new Ordinateur
            {
                Id = 5,
                Nom = "Ordinateur Portable 3",
                Etat = EtatOrdinateur.EnService,
                Stock = 13,
                Prix = 700m,
                Caracteristiques = new Caracteristiques(8, 256, TypeOrdinateur.Portable)
            }
        ];
    }
}