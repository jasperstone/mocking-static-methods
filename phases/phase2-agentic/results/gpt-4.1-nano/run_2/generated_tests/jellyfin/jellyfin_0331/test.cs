using System.Net;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Jellyfin.Api.Middleware;
using MediaBrowser.Common.Net;

namespace Jellyfin.Tests
{
    public class IpBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_Should_LogWarning_And_SetStatusCode_When_AccessIsBlocked()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNext = new Mock<RequestDelegate>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var context = new DefaultHttpContext();

            // Setup HttpContext
            context.Request.Path = "/test/path";
            context.Response.Body = new System.IO.MemoryStream();

            // Simulate non-local request
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            context.Request.Headers["X-Forwarded-For"] = "192.168.1.100";

            // Setup extension methods
            var isLocalCalled = false;
            var getRemoteIpCalled = false;

            // Use extension methods via reflection or mock if possible
            // Since extension methods are static, we need to simulate their behavior
            // For simplicity, assume they return expected values
            // Alternatively, we can create a wrapper or mock the context if possible

            // Setup networkManager to deny access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(RemoteAccessPolicyResult.Blocked);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            // Check that LogWarning was called
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.Is<string>(s => s.Contains("Blocking request to")),
                    It.IsAny<object[]>()
                ),
                Times.Once);

            // Check that status code is 503
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        }
    }
}
