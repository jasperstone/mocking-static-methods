using Jellyfin.Api.Middleware;
using Jellyfin.Api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Api.Tests
{
    public class IpBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_AllowAccess_LogsNothing()
        {
            // Arrange
            var next = new Mock<RequestDelegate>();
            var logger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManager = new Mock<INetworkManager>();
            networkManager.Setup(n => n.ShouldAllowServerAccess(It.IsAny<string>())).Returns(RemoteAccessPolicyResult.Allow);
            var httpContext = new DefaultHttpContext();
            var middleware = new IpBasedAccessValidationMiddleware(next.Object, logger.Object);

            // Act
            await middleware.Invoke(httpContext, networkManager.Object);

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_DenyAccess_LogsWarning()
        {
            // Arrange
            var next = new Mock<RequestDelegate>();
            var logger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManager = new Mock<INetworkManager>();
            networkManager.Setup(n => n.ShouldAllowServerAccess(It.IsAny<string>())).Returns(RemoteAccessPolicyResult.Deny);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/path";
            httpContext.Connection.RemoteIpAddress = new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 });
            var middleware = new IpBasedAccessValidationMiddleware(next.Object, logger.Object);

            // Act
            await middleware.Invoke(httpContext, networkManager.Object);

            // Assert
            logger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
