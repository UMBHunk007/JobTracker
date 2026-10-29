using System.Text.Json;
using JobTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class CandidateProfileController : ControllerBase
{
    private readonly CandidateProfileService _service;

    public CandidateProfileController(CandidateProfileService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
            return BadRequest(new { message = "Debes seleccionar un archivo PDF." });

        try
        {
            var profile = await _service.UploadAsync(file, cancellationToken);
            var skills = JsonSerializer.Deserialize<List<string>>(profile.SkillsJson)
                ?? new List<string>();

            return Ok(new
            {
                profile.Id,
                profile.FileName,
                profile.UploadedAt,
                skills,
                skillCount = skills.Count
            });
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrent(
        CancellationToken cancellationToken)
    {
        var profile = await _service.GetCurrentAsync(cancellationToken);

        if (profile is null)
            return NotFound(new { message = "Todavía no has cargado un CV." });

        var skills = JsonSerializer.Deserialize<List<string>>(profile.SkillsJson)
            ?? new List<string>();

        return Ok(new
        {
            profile.Id,
            profile.FileName,
            profile.UploadedAt,
            skills,
            skillCount = skills.Count
        });
    }
}
