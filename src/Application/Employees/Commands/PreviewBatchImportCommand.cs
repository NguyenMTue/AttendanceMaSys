using System.ComponentModel.DataAnnotations;
using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.DTOs;
using AttendanceMaSys.Domain.Enums;

namespace AttendanceMaSys.Application.Employees.Commands;

public record PreviewBatchImportCommand(List<BatchImportEmployeeDto> Employees) : IRequest<BatchImportPreviewDto>;

public class PreviewBatchImportCommandHandler : IRequestHandler<PreviewBatchImportCommand, BatchImportPreviewDto>
{
    private readonly IIdentityService _identityService;

    public PreviewBatchImportCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<BatchImportPreviewDto> Handle(PreviewBatchImportCommand request, CancellationToken cancellationToken)
    {
        var preview = new BatchImportPreviewDto
        {
            TotalRows = request.Employees.Count
        };

        var emailMapInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < request.Employees.Count; i++)
        {
            var item = request.Employees[i];
            var rowIndex = i + 1;
            var errors = new List<string>();

            // 1. Check required fields
            if (string.IsNullOrWhiteSpace(item.FirstName))
            {
                errors.Add("Tên (FirstName) không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(item.LastName))
            {
                errors.Add("Họ/Tên lót (LastName) không được để trống.");
            }
            if (string.IsNullOrWhiteSpace(item.Email))
            {
                errors.Add("Email không được để trống.");
            }
            else if (!new EmailAddressAttribute().IsValid(item.Email))
            {
                errors.Add($"Email '{item.Email}' không đúng định dạng.");
            }
            else
            {
                // Duplicate check in file
                if (!emailMapInFile.Add(item.Email))
                {
                    errors.Add($"Email '{item.Email}' bị trùng lặp trong file.");
                }
                else
                {
                    // Duplicate check in DB
                    var existsInDb = await _identityService.UserExistsAsync(item.Email);
                    if (existsInDb)
                    {
                        errors.Add($"Tài khoản Email '{item.Email}' đã tồn tại trong hệ thống.");
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(item.Password) || item.Password.Length < 6)
            {
                errors.Add("Mật khẩu không được để trống và phải từ 6 ký tự trở lên.");
            }

            // 2. Validate Gender Enum
            if (!Enum.TryParse<Gender>(item.Gender, true, out _))
            {
                errors.Add($"Giới tính '{item.Gender}' không hợp lệ (Hợp lệ: Male, Female, Other).");
            }

            // 3. Validate Department Enum
            if (!Enum.TryParse<Department>(item.Department, true, out _))
            {
                errors.Add($"Phòng ban '{item.Department}' không hợp lệ (Hợp lệ: IT, HR, Finance, Sales).");
            }

            // 4. Validate EmployeeType and details
            var empType = item.EmployeeType ?? "Employee";
            if (empType.Equals("Developer", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Band.HasValue && (item.Band.Value < 1 || item.Band.Value > 10))
                {
                    errors.Add($"Cấp độ Developer '{item.Band}' phải từ 1 đến 10.");
                }
            }
            else if (empType.Equals("QA", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Band.HasValue && (item.Band.Value < 1 || item.Band.Value > 10))
                {
                    errors.Add($"Cấp độ QA '{item.Band}' phải từ 1 đến 10.");
                }
            }
            else if (empType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(item.ManagerType) &&
                    !item.ManagerType.Equals("DepartmentManager", StringComparison.OrdinalIgnoreCase) &&
                    !item.ManagerType.Equals("GeneralManager", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Loại Quản lý '{item.ManagerType}' không hợp lệ (Hợp lệ: DepartmentManager, GeneralManager).");
                }
            }

            var isValid = errors.Count == 0;
            if (isValid)
            {
                preview.ValidRowsCount++;
            }
            else
            {
                preview.InvalidRowsCount++;
            }

            preview.Items.Add(new BatchImportPreviewItemDto
            {
                RowIndex = rowIndex,
                Employee = item,
                IsValid = isValid,
                Errors = errors
            });
        }

        return preview;
    }
}
