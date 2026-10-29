using System.Text.Json.Serialization;

namespace JobTracker.Api.Models;

public class ComputrabajoApiResponse
{
    [JsonPropertyName("o")]
    public ComputrabajoOfferData? Offer { get; set; }
}

public class ComputrabajoOfferData
{
    [JsonPropertyName("ltr")]
    public string? Title { get; set; }

    [JsonPropertyName("cn")]
    public string? Company { get; set; }

    [JsonPropertyName("c")]
    public string? Location { get; set; }

    [JsonPropertyName("ld")]
    public string? Description { get; set; }

    [JsonPropertyName("s")]
    public decimal Salary { get; set; }

    [JsonPropertyName("lss")]
    public string? SalaryText { get; set; }

    [JsonPropertyName("dlu")]
    public DateTime? PublishedAt { get; set; }
}
