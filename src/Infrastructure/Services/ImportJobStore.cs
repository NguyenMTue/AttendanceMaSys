using System.Collections.Concurrent;
using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Infrastructure.Services;

public class ImportJobStore : IImportJobStore
{
    private readonly ConcurrentDictionary<Guid, ImportJobStatusDto> _jobs = new();

    public Guid CreateJob(int totalRows)
    {
        var jobId = Guid.NewGuid();
        var job = new ImportJobStatusDto
        {
            JobId = jobId,
            Status = "Pending",
            TotalRows = totalRows,
            ProcessedRows = 0,
            SuccessCount = 0,
            FailedCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _jobs[jobId] = job;
        return jobId;
    }

    public ImportJobStatusDto? GetJob(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var job);
        return job;
    }

    public void UpdateJobStatus(Guid jobId, string status)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            lock (job)
            {
                job.Status = status;
            }
        }
    }

    public void IncrementProgress(Guid jobId, bool isSuccess, int rowIndex, string email, string? errorMessage = null)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            lock (job)
            {
                job.ProcessedRows++;
                if (isSuccess)
                {
                    job.SuccessCount++;
                }
                else
                {
                    job.FailedCount++;
                    job.Errors.Add(new BatchImportJobErrorDto
                    {
                        RowIndex = rowIndex,
                        Email = email,
                        ErrorMessage = errorMessage ?? "Unknown error"
                    });
                }
            }
        }
    }

    public void CompleteJob(Guid jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            lock (job)
            {
                job.Status = "Completed";
                job.CompletedAt = DateTime.UtcNow;
            }
        }
    }

    public void FailJob(Guid jobId, string reason)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            lock (job)
            {
                job.Status = "Failed";
                job.CompletedAt = DateTime.UtcNow;
                job.Errors.Add(new BatchImportJobErrorDto
                {
                    RowIndex = 0,
                    Email = "System",
                    ErrorMessage = reason
                });
            }
        }
    }
}
