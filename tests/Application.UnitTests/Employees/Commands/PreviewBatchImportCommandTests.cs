using AttendanceMaSys.Application.Common.Interfaces;
using AttendanceMaSys.Application.Employees.Commands;
using AttendanceMaSys.Application.Employees.DTOs;
using Moq;
using NUnit.Framework;

namespace AttendanceMaSys.Application.UnitTests.Employees.Commands;

[TestFixture]
public class PreviewBatchImportCommandTests
{
    private Mock<IIdentityService> _identityServiceMock = null!;

    [SetUp]
    public void Setup()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _identityServiceMock
            .Setup(x => x.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
    }

    [Test]
    public async Task Handle_ValidEmployees_ReturnsCanCommitTrue()
    {
        // Arrange
        var handler = new PreviewBatchImportCommandHandler(_identityServiceMock.Object);
        var employees = new List<BatchImportEmployeeDto>
        {
            new BatchImportEmployeeDto
            {
                FirstName = "Hòa",
                LastName = "Đặng",
                Email = "test1@company.com",
                Password = "Password123!",
                Gender = "Male",
                Department = "IT",
                EmployeeType = "Developer",
                Band = 2
            }
        };

        var command = new PreviewBatchImportCommand(employees);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.TotalRows, Is.EqualTo(1));
        Assert.That(result.ValidRowsCount, Is.EqualTo(1));
        Assert.That(result.InvalidRowsCount, Is.EqualTo(0));
        Assert.That(result.CanCommit, Is.True);
        Assert.That(result.Items[0].IsValid, Is.True);
    }

    [Test]
    public async Task Handle_InvalidAndDuplicateEmail_ReturnsErrorsPerMessage()
    {
        // Arrange
        _identityServiceMock
            .Setup(x => x.UserExistsAsync("existing@company.com"))
            .ReturnsAsync(true);

        var handler = new PreviewBatchImportCommandHandler(_identityServiceMock.Object);
        var employees = new List<BatchImportEmployeeDto>
        {
            new BatchImportEmployeeDto
            {
                FirstName = "",
                LastName = "Nguyễn",
                Email = "invalid_email_format",
                Password = "123",
                Gender = "InvalidGender",
                Department = "InvalidDept"
            },
            new BatchImportEmployeeDto
            {
                FirstName = "Văn",
                LastName = "A",
                Email = "existing@company.com",
                Password = "Password123!",
                Gender = "Male",
                Department = "IT"
            }
        };

        var command = new PreviewBatchImportCommand(employees);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.TotalRows, Is.EqualTo(2));
        Assert.That(result.ValidRowsCount, Is.EqualTo(0));
        Assert.That(result.InvalidRowsCount, Is.EqualTo(2));
        Assert.That(result.CanCommit, Is.False);

        Assert.That(result.Items[0].IsValid, Is.False);
        Assert.That(result.Items[0].Errors.Count, Is.GreaterThanOrEqualTo(4));

        Assert.That(result.Items[1].IsValid, Is.False);
        Assert.That(result.Items[1].Errors, Does.Contain("Tài khoản Email 'existing@company.com' đã tồn tại trong hệ thống."));
    }
}
