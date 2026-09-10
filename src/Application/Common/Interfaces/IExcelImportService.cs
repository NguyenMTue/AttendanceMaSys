using System.IO;
using AttendanceMaSys.Application.Employees.DTOs;

namespace AttendanceMaSys.Application.Common.Interfaces;

public interface IExcelImportService
{
    List<BatchImportEmployeeDto> ReadEmployeesFromExcel(Stream stream);
    byte[] GenerateExcelTemplate();
}
