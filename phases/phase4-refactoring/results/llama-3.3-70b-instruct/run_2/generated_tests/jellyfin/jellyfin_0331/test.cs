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
    public async Task Invoke_LogsWarningAndReturns503_WhenRemoteAccessIsNotAllowed()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(r => r.Path).Returns("/test");
        httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
        var responseMock = new Mock<HttpResponse>();
        responseMock.SetupGet(r => r.StatusCode).Returns(200);
        httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

        var middleware = new IPBasedAccessValidationMiddleware(async (httpContext) => { }, loggerMock.Object);

        // Act
        await middleware.Invoke(httpContextMock.Object, null).ConfigureAwait(false);

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        responseMock.VerifySet(r => r.StatusCode = StatusCodes.Status503ServiceUnavailable, Times.Once);
    }
}
