namespace GestionOrdinateurs.API;

public class StockService
{
    private readonly object verrou = new();
    private int stockSSD = 10;

    public int StockSSD
    {
        get
        {
            lock (verrou)
            {
                return stockSSD;
            }
        }
    }

    public void ConsommerSSD()
    {
        lock (verrou)
        {
            stockSSD--;
        }
    }
}
