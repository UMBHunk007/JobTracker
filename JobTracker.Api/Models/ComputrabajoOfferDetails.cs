namespace JobTracker.Api.Models;

public class ComputrabajoOfferDetails
{
    public string Title { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Salary { get; set; }
    public string SalaryText { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
}