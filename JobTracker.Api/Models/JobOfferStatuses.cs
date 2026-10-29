namespace JobTracker.Api.Models;

public static class JobOfferStatuses
{
    public const string Pending = "Pending";
    public const string Ready = "Ready";
    public const string Skipped = "Skipped";
    public const string Applied = "Applied";

    public static bool IsValid(string? status)
    {
        return status is Pending or Ready or Skipped or Applied;
    }
}
