using System.Threading.Tasks;
using Jellyfin.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Net;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ShouldLogWarning_WhenRequestIsBlocked()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                              .Returns(RemoteAccessPolicyResult.Denied);

            var next = new RequestDelegate((innerHttpContext) => Task.CompletedTask);
            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test";
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 });

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
