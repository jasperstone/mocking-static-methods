using System;
using System.Threading.Tasks;
using Jellyfin.Api.Middleware;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ShouldLogWarning_WhenAccessIsBlocked()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test";
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 });

            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(RemoteAccessPolicyResult.Denied);

            var middleware = new IPBasedAccessValidationMiddleware(next: (innerHttpContext) => Task.CompletedTask, logger: loggerMock.Object);

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocking request to /test by 192.168.1.1 due to IP filtering rule, reason: Denied")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_ShouldNotLogWarning_WhenAccessIsAllowed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test";
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 });

            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(RemoteAccessPolicyResult.Allow);

            var middleware = new IPBasedAccessValidationMiddleware(next: (innerHttpContext) => Task.CompletedTask, logger: loggerMock.Object);

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocking request to /test by 192.168.1.1 due to IP filtering rule, reason: Denied")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);

            Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        }
    }
}
