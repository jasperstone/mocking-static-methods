using Jellyfin.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Api.Tests.Middleware;

public class IPBasedAccessValidationMiddlewareTests
{
    [Fact]
    public async Task Invoke_LogsWarning_WhenRemoteAccessIsNotAllowed()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.IsLocal()).Returns(false);
        httpContextMock.Setup(c => c.GetNormalizedRemoteIP()).Returns(IPAddress.Parse("192.168.1.100"));
        httpContextMock.Setup(c => c.Request.Path).Returns("/path/to/resource");

        var middleware = new IPBasedAccessValidationMiddleware((httpContext) => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.Invoke(httpContextMock.Object, null).ConfigureAwait(false);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task Invoke_AllowsAccess_WhenRemoteAccessIsAllowed()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.IsLocal()).Returns(true);
        httpContextMock.Setup(c => c.GetNormalizedRemoteIP()).Returns(IPAddress.Parse("192.168.1.100"));
        httpContextMock.Setup(c => c.Request.Path).Returns("/path/to/resource");

        var middleware = new IPBasedAccessValidationMiddleware((httpContext) => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.Invoke(httpContextMock.Object, null).ConfigureAwait(false);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
