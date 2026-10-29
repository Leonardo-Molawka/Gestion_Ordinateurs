using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

[ApiController]
[Authorize]
[Route("api/reparations")]
public class ReparationController : ControllerBase
{
    private readonly IOrdinateurService ordinateurService;
    private readonly ReparationService reparationService;
    private readonly StockService stockService;
    private readonly ILogger<ReparationController> logger;

    public ReparationController(
        IOrdinateurService ordinateurService,
        ReparationService reparationService,
        StockService stockService,
        ILogger<ReparationController> logger)
    {
        this.ordinateurService = ordinateurService;
        this.reparationService = reparationService;
        this.stockService = stockService;
        this.logger = logger;
    }

    // POST /api/reparations/SimulerReparation
    [HttpPost("SimulerReparation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SimulerReparation(CancellationToken cancellationToken)
    {
        List<Ordinateur> listOrdinateurs =
            await ordinateurService.ListerOrdinateursAsync(cancellationToken);

        if (listOrdinateurs.Count < 5)
        {
            return BadRequest("Il faut au moins cinq ordinateurs pour simuler les réparations.");
        }

        await Task.WhenAll(
            listOrdinateurs
                .Take(5)
                .Select(ordinateur => reparationService.SimulerReparationAsync(
                    ordinateur,
                    cancellationToken)));

        return Ok();
    }

    // GET /api/reparations
    [HttpGet]
    public async Task<IActionResult> GetReparations(CancellationToken cancellationToken)
    {
        return Ok(await reparationService.ListerReparationsAsync(cancellationToken));
    }

    // GET /api/reparations/historique
    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique(CancellationToken cancellationToken)
    {
        return Ok(await reparationService.ObtenirHistoriqueAsync(cancellationToken));
    }

    // GET /api/reparations/stockSSD
    [HttpGet("GetStockSSD")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetStockSSD()
    {
        Parallel.For(0, 10, _ =>
        {
            Task.Delay(10).Wait();
            stockService.ConsommerSSD();
        });

        logger.LogInformation("Stock SSD restant : {StockSSD}", stockService.StockSSD);
        return Ok(stockService.StockSSD);
    }
}
