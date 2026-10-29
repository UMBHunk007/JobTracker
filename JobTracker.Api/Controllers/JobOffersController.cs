using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobOffersController : ControllerBase
{
    private readonly JobTrackerDbContext _context;

    public JobOffersController(JobTrackerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobOffer>>> GetAll(
        [FromQuery] decimal? minScore = null)
    {
        if (minScore is < 0 or > 100)
            return BadRequest(new
            {
                message = "El score mínimo debe estar entre 0 y 100."
            });

        var query = _context.JobOffers.AsQueryable();

        if (minScore.HasValue)
        {
            query = query.Where(o =>
                o.MatchScore.HasValue &&
                o.MatchScore.Value >= minScore.Value);
        }

        var offers = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(offers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobOffer>> GetById(int id)
    {
        var offer = await _context.JobOffers.FindAsync(id);

        if (offer is null)
            return NotFound();

        return Ok(offer);
    }

    [HttpPost]
    public async Task<ActionResult<JobOffer>> Create(JobOffer offer)
    {
        if (!JobOfferStatuses.IsValid(offer.Status))
        {
            return BadRequest(new
            {
                message = "Estado inválido.",
                allowedStatuses = new[]
                {
                    JobOfferStatuses.Pending,
                    JobOfferStatuses.Ready,
                    JobOfferStatuses.Skipped,
                    JobOfferStatuses.Applied
                }
            });
        }

        offer.Id = 0;
        offer.CreatedAt = DateTime.UtcNow;

        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = offer.Id },
            offer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, JobOffer updatedOffer)
    {
        var offer = await _context.JobOffers.FindAsync(id);

        if (offer is null)
            return NotFound();

        if (!JobOfferStatuses.IsValid(updatedOffer.Status))
        {
            return BadRequest(new
            {
                message = "Estado inválido.",
                allowedStatuses = new[]
                {
                    JobOfferStatuses.Pending,
                    JobOfferStatuses.Ready,
                    JobOfferStatuses.Skipped,
                    JobOfferStatuses.Applied
                }
            });
        }

        offer.Title = updatedOffer.Title;
        offer.Company = updatedOffer.Company;
        offer.Url = updatedOffer.Url;
        offer.Description = updatedOffer.Description;
        offer.MatchScore = updatedOffer.MatchScore;
        offer.Status = updatedOffer.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await _context.JobOffers.FindAsync(id);

        if (offer is null)
            return NotFound();

        _context.JobOffers.Remove(offer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
