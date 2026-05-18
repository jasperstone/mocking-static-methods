using System.Net;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Api.Middleware.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarning_WhenAccessIsDenied()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test-path";
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

            networkManagerMock
                .Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Deny);

            var middleware = new IPBasedAccessValidationMiddleware(
                async context => { await Task.CompletedTask; },
                loggerMock.Object);

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocking request to /test-path by 192.168.1.1 due to IP filtering rule, reason: Deny")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
