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
                .Returns(RemoteAccessPolicyResult.Deny);

            var middleware = new IPBasedAccessValidationMiddleware(
                next: (innerHttpContext) => Task.CompletedTask,
                logger: loggerMock.Object);

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
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

            var middleware = new IPBasedAccessValidationMiddleware(
                next: (innerHttpContext) => Task.CompletedTask,
                logger: loggerMock.Object);

            // Act
            await middleware.Invoke(httpContext, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);

            Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        }
    }
}
