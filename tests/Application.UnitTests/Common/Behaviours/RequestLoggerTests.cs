using AttendanceMaSys.Application.Common.Behaviours;
using AttendanceMaSys.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AttendanceMaSys.Application.UnitTests.Common.Behaviours;

public record TestRequestCommand : IRequest;

public class RequestLoggerTests
{
    private Mock<ILogger<TestRequestCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<TestRequestCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<TestRequestCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new TestRequestCommand(), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<TestRequestCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new TestRequestCommand(), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
