using System.Text.RegularExpressions;
using JobTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComputrabajoController : ControllerBase
{
    private readonly ComputrabajoSearchService _service;
    private readonly ComputrabajoImportService _importService;

    public ComputrabajoController(
        ComputrabajoSearchService service,
        ComputrabajoImportService importService)
    {
        _service = service;
        _importService = importService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string title = "desarrollador",
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Debes indicar un título de búsqueda." });

        if (page < 1)
            return BadRequest(new { message = "La página debe ser mayor que cero." });

        try
        {
            var results = await _service.SearchAsync(title, page, cancellationToken);
            return Ok(results);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "No fue posible consultar Computrabajo."
            });
        }
    }

    [HttpGet("details/{externalId}")]
    public async Task<IActionResult> GetDetails(
        string externalId,
        CancellationToken cancellationToken)
    {
        if (!Regex.IsMatch(externalId, @"^[A-Fa-f0-9]{32}$"))
            return BadRequest(new { message = "Identificador de oferta inválido." });

        try
        {
            var details = await _service.GetDetailsAsync(
                externalId, cancellationToken);

            if (details is null)
                return NotFound();

            return Ok(details);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "No fue posible consultar los detalles en Computrabajo."
            });
        }
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(
        [FromQuery] string title = "desarrollador",
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            return BadRequest(new { message = "Debes indicar un título de búsqueda." });

        if (page < 1)
            return BadRequest(new { message = "La página debe ser mayor que cero." });

        try
        {
            var result = await _importService.ImportAsync(
                title, page, cancellationToken);

            return Ok(result);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new
            {
                message = "No fue posible consultar Computrabajo."
            });
        }
    }

    [HttpPost("recalculate-scores")]
    public async Task<IActionResult> RecalculateScores(
        CancellationToken cancellationToken)
    {
        var updated = await _importService.RecalculateScoresAsync(
            cancellationToken);

        if (updated is null)
        {
            return BadRequest(new
            {
                message = "Primero debes cargar un CV."
            });
        }

        return Ok(new
        {
            updated,
            message = "Scores recalculados correctamente."
        });
    }
}
