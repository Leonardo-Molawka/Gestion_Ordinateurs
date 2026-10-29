namespace GestionOrdinateurs.API;

public class JournalService
{
    private static readonly object verrouFichier = new();

    public void Ecrire(string message)
    {
        EcrireDansFichier(Path.Combine("Logs", "Reparations"), "reparations.log", message);
    }

    public void EcrireOrdinateur(string message)
    {
        EcrireDansFichier(Path.Combine("Logs", "Ordinateurs"), "ordinateurs.log", message);
    }

    private static void EcrireDansFichier(string repertoire, string nomFichier, string message)
    {
        Directory.CreateDirectory(repertoire);
        string messageAvecDate = $"{DateTime.Now:dd/MM/yyyy} - {message}{Environment.NewLine}";
        lock (verrouFichier)
        {
            File.AppendAllText(Path.Combine(repertoire, nomFichier), messageAvecDate);
        }
    }
}
