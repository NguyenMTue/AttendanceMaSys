namespace AttendanceMaSys.Application.Employees.DTOs;

public class BatchImportJobErrorDto
{
    public int RowIndex { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class ImportJobStatusDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public double ProgressPercentage => TotalRows > 0 ? Math.Round((double)ProcessedRows / TotalRows * 100, 2) : 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<BatchImportJobErrorDto> Errors { get; set; } = new();
}
