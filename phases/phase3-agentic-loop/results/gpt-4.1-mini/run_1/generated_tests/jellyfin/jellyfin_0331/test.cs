using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using MediaBrowser.Common.Net;
using System.Threading;
using System.Web;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LocalRequest_CallsNextDelegate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var calledNext = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            // Setup IsLocal extension method to return true
            context.Connection.RemoteIpAddress = IPAddress.Loopback;

            // We need to mock the IsLocal extension method, but since it's an extension method,
            // we will simulate by setting RemoteIpAddress to loopback and rely on the actual implementation.
            // The actual IsLocal method is from MediaBrowser.Common.Extensions, but we assume it checks for loopback.

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.True(calledNext);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_RemoteRequest_Allowed_CallsNextDelegate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var calledNext = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("1.2.3.4");

            // Setup IsLocal extension method to return false by simulating non-local IP
            // We assume IsLocal returns false for non-loopback IPs.

            // Setup GetNormalizedRemoteIP extension method to return the RemoteIpAddress
            // We simulate by just using the RemoteIpAddress property directly.

            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Allow);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.True(calledNext);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_RemoteRequest_NotAllowed_LogsWarningAndReturns503()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var calledNext = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            var remoteIp = IPAddress.Parse("5.6.7.8");
            context.Connection.RemoteIpAddress = remoteIp;
            context.Request.Path = "/test/path";

            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.RejectDueToIPBlocklist);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.False(calledNext);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            // Verify the LogWarning call with expected message and parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Blocking request to") &&
                        v.ToString().Contains(HttpUtility.UrlEncode(context.Request.Path)) &&
                        v.ToString().Contains(remoteIp.ToString()) &&
                        v.ToString().Contains(RemoteAccessPolicyResult.RejectDueToIPBlocklist.ToString())
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
