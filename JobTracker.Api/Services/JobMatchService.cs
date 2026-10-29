using System.Text.RegularExpressions;

namespace JobTracker.Api.Services;

public class JobMatchService
{
    private sealed record SkillRule(int Weight, string[] Aliases);

    private static readonly Dictionary<string, SkillRule> SkillRules =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["C#"] = new(4, new[] { "C#", "C Sharp" }),
            ["C++"] = new(3, new[] { "C++" }),
            ["Java"] = new(3, new[] { "Java" }),
            ["JavaScript"] = new(3, new[] { "JavaScript" }),
            ["TypeScript"] = new(3, new[] { "TypeScript" }),
            ["Python"] = new(3, new[] { "Python" }),
            ["PHP"] = new(3, new[] { "PHP" }),
            ["Objective-C"] = new(3, new[] { "Objective-C", "Objective C" }),
            ["Swift"] = new(3, new[] { "Swift" }),
            [".NET"] = new(4, new[] { ".NET", "dotnet" }),
            ["ASP.NET Core"] = new(5, new[] { "ASP.NET Core", "ASP.NET" }),
            ["REST APIs"] = new(4, new[] { "REST API", "REST APIs", "RESTful API" }),
            ["SQL Server"] = new(4, new[] { "SQL Server", "MSSQL", "Microsoft SQL Server" }),
            ["SQL"] = new(3, new[] { "SQL" }),
            ["PL/SQL"] = new(4, new[] { "PL/SQL" }),
            ["Oracle"] = new(4, new[] { "Oracle" }),
            ["PostgreSQL"] = new(4, new[] { "PostgreSQL" }),
            ["MySQL"] = new(4, new[] { "MySQL" }),
            ["MongoDB"] = new(3, new[] { "MongoDB" }),
            ["Redis"] = new(3, new[] { "Redis" }),
            ["Docker"] = new(3, new[] { "Docker" }),
            ["Docker Compose"] = new(2, new[] { "Docker Compose" }),
            ["Kubernetes"] = new(3, new[] { "Kubernetes", "K8s" }),
            ["AWS"] = new(3, new[] { "AWS", "Amazon Web Services" }),
            ["GCP"] = new(3, new[] { "GCP", "Google Cloud Platform" }),
            ["Azure"] = new(3, new[] { "Azure" }),
            ["Jenkins"] = new(2, new[] { "Jenkins" }),
            ["GitLab CI/CD"] = new(2, new[] { "GitLab CI/CD" }),
            ["CI/CD"] = new(2, new[] { "CI/CD" }),
            ["Git"] = new(1, new[] { "Git" }),
            ["Unity"] = new(2, new[] { "Unity" }),
            ["React"] = new(3, new[] { "React", "React.js" }),
            ["Angular"] = new(3, new[] { "Angular" }),
            ["Node.js"] = new(3, new[] { "Node.js", "NodeJS" }),
            ["Laravel"] = new(3, new[] { "Laravel" }),
            ["JHipster"] = new(2, new[] { "JHipster" }),
            ["Linux"] = new(2, new[] { "Linux", "CentOS" }),
            ["Microservices"] = new(3, new[] { "Microservices", "Microservicios" })
        };

    public decimal CalculateScore(
        IEnumerable<string> candidateSkills,
        string? title,
        string? description)
    {
        var profileSkills = new HashSet<string>(
            candidateSkills, StringComparer.OrdinalIgnoreCase);

        var jobText = $"{title ?? string.Empty}\n{description ?? string.Empty}";

        var requiredSkills = SkillRules
            .Where(rule => rule.Value.Aliases.Any(alias =>
                ContainsKeyword(jobText, alias)))
            .ToList();

        if (requiredSkills.Count == 0)
            return 0m;

        var totalWeight = requiredSkills.Sum(rule => rule.Value.Weight);

        var matchedWeight = requiredSkills
            .Where(rule => profileSkills.Contains(rule.Key))
            .Sum(rule => rule.Value.Weight);

        return Math.Round(matchedWeight * 100m / totalWeight, 2);
    }

    private static bool ContainsKeyword(string text, string keyword)
    {
        return Regex.IsMatch(
            text,
            @"(?<![\p{L}\p{N}/])" +
            Regex.Escape(keyword) +
            @"(?![\p{L}\p{N}])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
