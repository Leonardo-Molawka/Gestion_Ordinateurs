namespace GestionOrdinateurs.Shared;

public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }

    public Caracteristiques()
    {
        RamGo = 0;
        StockageGo = 0;
        TypeOrdi = TypeOrdinateur.Portable;
    }

    public Caracteristiques(int ramGo, int stockageGo, TypeOrdinateur typeOrdi)
    {
        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
    }
}
