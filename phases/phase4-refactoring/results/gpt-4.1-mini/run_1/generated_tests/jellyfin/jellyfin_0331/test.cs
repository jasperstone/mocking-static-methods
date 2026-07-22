using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using System.Net;
using System.Web;
using MediaBrowser.Common.Net;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LocalRequest_CallsNextAndDoesNotLogWarning()
        {
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var mockHttpContext = new DefaultHttpContext();
            var calledNext = false;

            // Setup IsLocal extension method to return true
            // We cannot mock extension methods directly, so we simulate by setting RemoteIpAddress to loopback
            mockHttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            await middleware.Invoke(mockHttpContext, mockNetworkManager.Object);

            Assert.True(calledNext);
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<System.Exception>(),
                    (Func<It.IsAnyType, System.Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task Invoke_RemoteRequestAllowed_CallsNextAndDoesNotLogWarning()
        {
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var mockHttpContext = new DefaultHttpContext();
            var calledNext = false;

            // Setup RemoteIpAddress to non-local IP
            mockHttpContext.Connection.RemoteIpAddress = IPAddress.Parse("1.2.3.4");
            // Setup Request.Path
            mockHttpContext.Request.Path = "/testpath";

            // Setup networkManager to allow access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Allow);

            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            await middleware.Invoke(mockHttpContext, mockNetworkManager.Object);

            Assert.True(calledNext);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, mockHttpContext.Response.StatusCode);
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<System.Exception>(),
                    (Func<It.IsAnyType, System.Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task Invoke_RemoteRequestDenied_LogsWarningAndSets503()
        {
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var mockHttpContext = new DefaultHttpContext();

            // Setup RemoteIpAddress to non-local IP
            var remoteIp = IPAddress.Parse("1.2.3.4");
            mockHttpContext.Connection.RemoteIpAddress = remoteIp;
            mockHttpContext.Request.Path = "/testpath";

            // Setup networkManager to deny access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Deny);

            RequestDelegate next = (ctx) =>
            {
                // Should not be called
                Assert.False(true, "Next delegate should not be called when access denied");
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            await middleware.Invoke(mockHttpContext, mockNetworkManager.Object);

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, mockHttpContext.Response.StatusCode);

            // Verify LogWarning was called once
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
