namespace JobTracker.Api.Models;

public class ComputrabajoImportResult
{
    public int Found { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
}
