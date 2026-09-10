using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Application.Common.Interfaces;

public interface IImportJobStore
{
    Guid CreateJob(int totalRows);
    ImportJobStatusDto? GetJob(Guid jobId);
    void UpdateJobStatus(Guid jobId, string status);
    void IncrementProgress(Guid jobId, bool isSuccess, int rowIndex, string email, string? errorMessage = null);
    void CompleteJob(Guid jobId);
    void FailJob(Guid jobId, string reason);
}
