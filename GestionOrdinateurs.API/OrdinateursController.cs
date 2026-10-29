using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService service;
    private readonly ILogger<OrdinateursController> logger;
    private readonly IConfiguration configuration;
    public OrdinateursController(
        IOrdinateurService service,
        ILogger<OrdinateursController> logger,
        IConfiguration configuration)
    {
        this.service = service;
        this.logger = logger;
        this.configuration = configuration;

        service.OrdinateurAjoute += AfficherMessageOrdinateurAjoute;
        service.OrdinateurSupprime += AfficherMessageOrdinateurSupprime;
        service.OrdinateurMisAJour += AfficherMessageOrdinateurMisAJour;
    }

    private void AfficherMessageOrdinateurAjoute(object? sender, OrdinateurAjouteEventArgs e)
    {
        logger.LogInformation("Nouvel ordinateur ajouté : {Nom}", e.Ordinateur.Nom);
    }

    private void AfficherMessageOrdinateurSupprime(object? sender, OrdinateurSupprimeEventArgs e)
    {
        logger.LogInformation("Ordinateur supprimé : {Nom}", e.Ordinateur.Nom);
    }

    private void AfficherMessageOrdinateurMisAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        logger.LogInformation("Ordinateur mis à jour : {Nom}", e.Ordinateur.Nom);
    }
    
    // GET /api/ordinateurs
    [HttpGet]
    public async Task<IActionResult> GetOrdinateurs(CancellationToken cancellationToken)
    {
        logger.LogInformation("Récupération de la liste des ordinateurs");
        return Ok(await service.ListerOrdinateursAsync(cancellationToken));
    }

    // GET /api/ordinateurs/disponibles
    [HttpGet("disponibles")]
    public async Task<IActionResult> GetOrdinateursDisponibles(CancellationToken cancellationToken)
    {
        return Ok(await service.ListerOrdinateursDisponiblesAsync(cancellationToken));
    }

    // GET /api/ordinateurs/prix
    [HttpGet("prix")]
    public async Task<IActionResult> GetOrdinateursParPrixDecroissant(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ListerParPrixDecroissantAsync(cancellationToken));
    }

    // GET /api/ordinateurs/statistiques
    [HttpGet("statistiques")]
    public IActionResult GetStatistiques()
    {
        var (total, disponibles, valeurStock, nbReparations) = service.CalculerStatistiques();
        return Ok(new
        {
            TotalOrdinateur = total,
            OrdiDisponibles = disponibles,
            ValeurStock = valeurStock,
            NbReparations = nbReparations
        });
    }

    // GET /api/ordinateurs/inventaire/{nom}
    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> ChercherInventaire(string nom, CancellationToken cancellationToken)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await service.ListerOrdinateursAsync(cancellationToken)
        };

        return Ok(inventaire[nom]);
    }

    // GET /api/ordinateurs/{id}/diagnostic
    [HttpGet("{id}/diagnostic")]
    public async Task<IActionResult> GetDiagnostic(int id, CancellationToken cancellationToken)
    {
        Ordinateur? ordinateur = await service.RenvoyerOrdinateurParIdAsync(id, cancellationToken);
        if (ordinateur == null)
        {
            return NotFound();
        }

        return Ok(new { Id = ordinateur.Id, Diagnostic = service.Diagnostiquer(ordinateur) });
    }

    // GET /api/ordinateurs/info
    [HttpGet("info")]
    public IActionResult GetApplicationInfo()
    {
        logger.LogInformation("Récupération des informations de l'application");
        var applicationInfo = new
        {
            Name = configuration["Application:Name"],
            Version = configuration["Application:Version"]
        };

        return Ok(applicationInfo);
    }

    // GET /api/ordinateurs/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComputerById(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Recherche de l'ordinateur {OrdinateurId}", id);
        Ordinateur? ordi = await service.RenvoyerOrdinateurParIdAsync(id, cancellationToken);
        
        if (ordi == null)
        {
            return NotFound(); 
        }
        return Ok(ordi);
    }

    // POST /api/ordinateurs
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AjouterOrdinateur(
        Ordinateur ordinateur,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Ajout d'un ordinateur");
        await service.AjouterOrdinateurAsync(ordinateur, cancellationToken);

        return CreatedAtAction(nameof(GetComputerById), new { id = ordinateur.Id }, ordinateur);
    }

    // PUT /api/ordinateurs/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ModifierOrdinateur(
        int id,
        Ordinateur ordinateur,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Mise à jour de l'ordinateur {OrdinateurId}", id);
        if (!await service.MiseAJourOrdinateurAsync(id, ordinateur, cancellationToken))
        {
            return NotFound();
        }

        return Ok();
    }

    // DELETE /api/ordinateurs/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SupprimerOrdinateur(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Suppression de l'ordinateur {OrdinateurId}", id);
        if (!await service.SupprimerOrdinateurAsync(id, cancellationToken))
        {
            return NotFound();
        }
        
        return NoContent(); 
    }
}