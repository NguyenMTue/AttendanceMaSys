namespace AttendanceMaSys.Application.Employees.DTOs;

public class BatchImportPreviewDto
{
    public int TotalRows { get; set; }
    public int ValidRowsCount { get; set; }
    public int InvalidRowsCount { get; set; }
    public bool CanCommit => ValidRowsCount > 0;
    public List<BatchImportPreviewItemDto> Items { get; set; } = new();
}
