using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Models;

public class JobOffer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Source { get; set; } = "Manual";

    [MaxLength(255)]
    public string? ExternalId { get; set; }

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public decimal? Salary { get; set; }

    public DateTime? PublishedAt { get; set; }

    public decimal? MatchScore { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = JobOfferStatuses.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}
