namespace AttendanceMaSys.Application.Employees.DTOs;

public class BatchImportPreviewItemDto
{
    public int RowIndex { get; set; }
    public BatchImportEmployeeDto Employee { get; set; } = new();
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
