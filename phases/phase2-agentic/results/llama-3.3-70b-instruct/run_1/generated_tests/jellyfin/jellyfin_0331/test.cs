using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Jellyfin.Api.Middleware;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Api.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarning_WhenRemoteAccessPolicyResultIsNotAllow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            var responseMock = new Mock<HttpResponse>();

            httpContextMock.Setup(c => c.IsLocal()).Returns(false);
            httpContextMock.Setup(c => c.GetNormalizedRemoteIP()).Returns("192.168.1.100");
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            requestMock.Setup(r => r.Path).Returns("/path");

            networkManagerMock.Setup(n => n.ShouldAllowServerAccess("192.168.1.100")).Returns(RemoteAccessPolicyResult.Deny);

            var middleware = new IPBasedAccessValidationMiddleware(async (context) => { }, loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            responseMock.Verify(r => r.StatusCode = StatusCodes.Status503ServiceUnavailable, Times.Once);
        }
    }
}
