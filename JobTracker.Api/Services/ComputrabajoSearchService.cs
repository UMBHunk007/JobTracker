using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Json;
using HtmlAgilityPack;
using JobTracker.Api.Models;

namespace JobTracker.Api.Services;

public class ComputrabajoSearchService
{
    private const string BaseUrl = "https://cl.computrabajo.com";
    private readonly HttpClient _httpClient;

    public ComputrabajoSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd(
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
            "es-CL,es;q=0.9,en;q=0.8");
    }

    public async Task<List<JobSearchResult>> SearchAsync(
        string title,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var slug = Regex.Replace(
            title.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

        var url = $"{BaseUrl}/trabajo-de-{Uri.EscapeDataString(slug)}?p={page}&by=pub_date";
        var html = await _httpClient.GetStringAsync(url, cancellationToken);

        return ParseHtml(html);
    }

    public async Task<ComputrabajoOfferDetails?> GetDetailsAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://oferta.computrabajo.com/offer/{externalId}/d/j?ipo=4&iapo=1";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var data = await JsonSerializer.DeserializeAsync<ComputrabajoApiResponse>(
            stream, cancellationToken: cancellationToken);

        var offer = data?.Offer;

        if (offer is null)
            return null;

        return new ComputrabajoOfferDetails
        {
            Title = offer.Title ?? string.Empty,
            Company = offer.Company ?? string.Empty,
            Location = offer.Location ?? string.Empty,
            Description = offer.Description ?? string.Empty,
            Salary = offer.Salary > 0 ? offer.Salary : null,
            SalaryText = offer.SalaryText ?? string.Empty,
            PublishedAt = DateTime.TryParse(
                offer.PublishedAt?.ToString("O"),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var publishedAt)
                    ? publishedAt
                    : null
        };
    }

    public List<JobSearchResult> ParseHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var links = document.DocumentNode.SelectNodes(
            "//a[contains(concat(' ', normalize-space(@class), ' '), ' js-o-link ')]");

        var results = new List<JobSearchResult>();

        if (links is null)
            return results;

        foreach (var link in links)
        {
            var href = link.GetAttributeValue("href", "");

            if (string.IsNullOrWhiteSpace(href))
                continue;

            var match = Regex.Match(href, @"-([A-Fa-f0-9]{32})(?:#|$)");

            if (!match.Success)
                continue;

            results.Add(new JobSearchResult
            {
                Title = HtmlEntity.DeEntitize(link.InnerText).Trim(),
                Url = new Uri(new Uri(BaseUrl), href).GetLeftPart(UriPartial.Path),
                ExternalId = match.Groups[1].Value
            });
        }

        return results
            .DistinctBy(result => result.ExternalId)
            .ToList();
    }
}
