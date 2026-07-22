using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using MediaBrowser.Common.Net;
using System.Web;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarningAndReturns503_WhenAccessDenied()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var context = new DefaultHttpContext();

            // Setup HttpContext to simulate non-local request
            // We cannot mock extension method IsLocal, so we simulate by setting RemoteIpAddress to non-local IP
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            context.Request.Path = "/testpath";

            // Setup INetworkManager.ShouldAllowServerAccess to return Deny
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess("192.168.1.100"))
                .Returns(RemoteAccessPolicyResult.Deny);

            // Setup next delegate
            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            // Verify logger.LogWarning was called with expected message and parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Blocking request to") &&
                        v.ToString().Contains(HttpUtility.UrlEncode(context.Request.Path)) &&
                        v.ToString().Contains("192.168.1.100") &&
                        v.ToString().Contains("Deny")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
