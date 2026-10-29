using System.Text.Json;
using System.Text.RegularExpressions;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace JobTracker.Api.Services;

public class CandidateProfileService
{
    private readonly JobTrackerDbContext _context;

    private static readonly (string Name, string[] Aliases)[] SkillsCatalog =
    {
        ("C#", new[] { "C#", "C Sharp" }),
        ("C++", new[] { "C++" }),
        ("Java", new[] { "Java" }),
        ("JavaScript", new[] { "JavaScript" }),
        ("TypeScript", new[] { "TypeScript" }),
        ("Python", new[] { "Python" }),
        ("PHP", new[] { "PHP" }),
        ("Objective-C", new[] { "Objective-C", "Objective C" }),
        ("Swift", new[] { "Swift" }),
        (".NET", new[] { ".NET", "dotnet" }),
        ("ASP.NET Core", new[] { "ASP.NET Core", "ASP.NET" }),
        ("REST APIs", new[] { "REST API", "REST APIs", "RESTful API" }),
        ("SQL Server", new[] { "SQL Server", "MSSQL" }),
        ("SQL", new[] { "SQL" }),
        ("PL/SQL", new[] { "PL/SQL" }),
        ("Oracle", new[] { "Oracle" }),
        ("PostgreSQL", new[] { "PostgreSQL" }),
        ("MySQL", new[] { "MySQL" }),
        ("MongoDB", new[] { "MongoDB" }),
        ("Redis", new[] { "Redis" }),
        ("Docker", new[] { "Docker" }),
        ("Docker Compose", new[] { "Docker Compose" }),
        ("Kubernetes", new[] { "Kubernetes", "K8s" }),
        ("AWS", new[] { "AWS", "Amazon Web Services" }),
        ("GCP", new[] { "GCP", "Google Cloud Platform" }),
        ("Azure", new[] { "Azure" }),
        ("Jenkins", new[] { "Jenkins" }),
        ("GitLab CI/CD", new[] { "GitLab CI/CD" }),
        ("CI/CD", new[] { "CI/CD" }),
        ("Git", new[] { "Git" }),
        ("Unity", new[] { "Unity" }),
        ("React", new[] { "React", "React.js" }),
        ("Angular", new[] { "Angular" }),
        ("Node.js", new[] { "Node.js", "NodeJS" }),
        ("Laravel", new[] { "Laravel" }),
        ("JHipster", new[] { "JHipster" }),
        ("Linux", new[] { "Linux", "CentOS" }),
        ("Microservices", new[] { "Microservices", "Microservicios" })
    };

    public CandidateProfileService(JobTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateProfile> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            throw new InvalidDataException("El archivo está vacío.");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidDataException("El tamaño máximo permitido es 5 MB.");

        if (!Path.GetExtension(file.FileName).Equals(
            ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Debes seleccionar un archivo PDF.");
        }

        string text;

        await using (var stream = file.OpenReadStream())
        {
            var header = new byte[5];
            var bytesRead = await stream.ReadAsync(
                header.AsMemory(0, 5), cancellationToken);

            if (bytesRead != 5 ||
                System.Text.Encoding.ASCII.GetString(header) != "%PDF-")
            {
                throw new InvalidDataException("El archivo no es un PDF válido.");
            }

            stream.Position = 0;

            try
            {
                using var document = PdfDocument.Open(stream);

                if (document.NumberOfPages > 30)
                    throw new InvalidDataException(
                        "El CV no puede superar las 30 páginas.");

                text = string.Join(
                    "\n",
                    document.GetPages().Select(page => page.Text));
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    "No se pudo leer el PDF. Comprueba que no esté dañado.",
                    ex);
            }
        }

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidDataException(
                "El PDF no contiene texto extraíble. Si es escaneado, necesitará OCR.");

        var skills = ExtractSkills(text);

        if (skills.Count == 0)
            throw new InvalidDataException(
                "No se reconocieron habilidades del catálogo en el CV.");

        var profile = await _context.CandidateProfiles
            .OrderByDescending(p => p.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            profile = new CandidateProfile();
            _context.CandidateProfiles.Add(profile);
        }

        profile.FileName = Path.GetFileName(file.FileName);
        profile.SkillsJson = JsonSerializer.Serialize(skills);
        profile.UploadedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return profile;
    }

    public async Task<CandidateProfile?> GetCurrentAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.CandidateProfiles
            .OrderByDescending(p => p.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static List<string> ExtractSkills(string text)
    {
        return SkillsCatalog
            .Where(skill => skill.Aliases.Any(alias =>
                Regex.IsMatch(
                    text,
                    @"(?<![\p{L}\p{N}])" +
                    Regex.Escape(alias) +
                    @"(?![\p{L}\p{N}])",
                    RegexOptions.IgnoreCase)))
            .Select(skill => skill.Name)
            .Distinct()
            .ToList();
    }
}
