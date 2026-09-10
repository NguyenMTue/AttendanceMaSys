using AttendanceMaSys.Application.Common.Interfaces;

namespace AttendanceMaSys.Application.Employees.Queries;

public record GetBatchImportTemplateQuery : IRequest<byte[]>;

public class GetBatchImportTemplateQueryHandler : IRequestHandler<GetBatchImportTemplateQuery, byte[]>
{
    private readonly IExcelImportService _excelImportService;

    public GetBatchImportTemplateQueryHandler(IExcelImportService excelImportService)
    {
        _excelImportService = excelImportService;
    }

    public Task<byte[]> Handle(GetBatchImportTemplateQuery request, CancellationToken cancellationToken)
    {
        var bytes = _excelImportService.GenerateExcelTemplate();
        return Task.FromResult(bytes);
    }
}
