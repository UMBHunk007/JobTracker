using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public string SkillsJson { get; set; } = "[]";

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
