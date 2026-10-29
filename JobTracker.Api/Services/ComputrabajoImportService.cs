using System.Text.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

public class ComputrabajoImportService
{
    private const string Source = "Computrabajo";

    private readonly JobTrackerDbContext _context;
    private readonly ComputrabajoSearchService _searchService;
    private readonly JobMatchService _matchService;

    public ComputrabajoImportService(
        JobTrackerDbContext context,
        ComputrabajoSearchService searchService,
        JobMatchService matchService)
    {
        _context = context;
        _searchService = searchService;
        _matchService = matchService;
    }

    public async Task<ComputrabajoImportResult> ImportAsync(
        string title,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var searchResults = await _searchService.SearchAsync(
            title, page, cancellationToken);

        var result = new ComputrabajoImportResult
        {
            Found = searchResults.Count
        };

        var profile = await _context.CandidateProfiles
            .OrderByDescending(p => p.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);

        List<string>? skills = profile is null
            ? null
            : JsonSerializer.Deserialize<List<string>>(profile.SkillsJson)
                ?? new List<string>();

        foreach (var item in searchResults)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var details = await _searchService.GetDetailsAsync(
                    item.ExternalId, cancellationToken);

                if (details is null)
                {
                    result.Failed++;
                    continue;
                }

                var offer = await _context.JobOffers.FirstOrDefaultAsync(
                    o => o.Source == Source &&
                         o.ExternalId == item.ExternalId,
                    cancellationToken);

                if (offer is null)
                {
                    offer = await _context.JobOffers.FirstOrDefaultAsync(
                        o => o.Source == Source && o.Url == item.Url,
                        cancellationToken);
                }

                var now = DateTime.UtcNow;
                decimal? score = skills is null
                    ? null
                    : _matchService.CalculateScore(
                        skills, details.Title, details.Description);

                if (offer is null)
                {
                    offer = new JobOffer
                    {
                        Source = Source,
                        ExternalId = item.ExternalId,
                        Url = item.Url,
                        Title = details.Title,
                        Company = details.Company,
                        Location = details.Location,
                        Description = details.Description,
                        Salary = details.Salary,
                        PublishedAt = details.PublishedAt,
                        MatchScore = score,
                        Status = JobOfferStatuses.Pending,
                        CreatedAt = now,
                        LastSeenAt = now
                    };

                    _context.JobOffers.Add(offer);
                    result.Inserted++;
                }
                else
                {
                    offer.ExternalId = item.ExternalId;
                    offer.Url = item.Url;
                    offer.Title = details.Title;
                    offer.Company = details.Company;
                    offer.Location = details.Location;
                    offer.Description = details.Description;
                    offer.Salary = details.Salary;
                    offer.PublishedAt = details.PublishedAt;
                    offer.LastSeenAt = now;

                    if (score.HasValue)
                        offer.MatchScore = score.Value;

                    result.Updated++;
                }
            }
            catch (Exception ex) when (
                ex is HttpRequestException or JsonException)
            {
                result.Failed++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<int?> RecalculateScoresAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await _context.CandidateProfiles
            .OrderByDescending(p => p.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
            return null;

        var skills = JsonSerializer.Deserialize<List<string>>(
            profile.SkillsJson) ?? new List<string>();

        var offers = await _context.JobOffers.ToListAsync(cancellationToken);

        foreach (var offer in offers)
        {
            offer.MatchScore = _matchService.CalculateScore(
                skills, offer.Title, offer.Description);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return offers.Count;
    }
}
