using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Application.Employees.Queries;

public record GetImportJobStatusQuery(Guid JobId) : IRequest<ImportJobStatusDto?>;

public class GetImportJobStatusQueryHandler : IRequestHandler<GetImportJobStatusQuery, ImportJobStatusDto?>
{
    private readonly IImportJobStore _importJobStore;

    public GetImportJobStatusQueryHandler(IImportJobStore importJobStore)
    {
        _importJobStore = importJobStore;
    }

    public Task<ImportJobStatusDto?> Handle(GetImportJobStatusQuery request, CancellationToken cancellationToken)
    {
        var job = _importJobStore.GetJob(request.JobId);
        return Task.FromResult(job);
    }
}
