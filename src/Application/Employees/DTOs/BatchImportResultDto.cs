namespace MindVaultAI.Application.Employees.DTOs;

public class BatchImportResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
