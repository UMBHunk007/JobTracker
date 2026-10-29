using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

public class JobTrackerDbContext : DbContext
{
    public JobTrackerDbContext(
        DbContextOptions<JobTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobOffer> JobOffers => Set<JobOffer>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobOffer>()
            .Property(offer => offer.MatchScore)
            .HasPrecision(5, 2);

        modelBuilder.Entity<JobOffer>()
            .Property(offer => offer.Salary)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JobOffer>()
            .HasIndex(offer => new { offer.Source, offer.ExternalId })
            .IsUnique()
            .HasFilter("[ExternalId] IS NOT NULL");

        modelBuilder.Entity<JobOffer>()
            .HasIndex(offer => new { offer.Source, offer.Url })
            .IsUnique();
    }
}
