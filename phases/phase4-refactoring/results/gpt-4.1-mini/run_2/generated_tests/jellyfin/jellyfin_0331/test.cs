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
        public async Task Invoke_LocalRequest_CallsNextAndDoesNotLogWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var calledNext = false;
            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            // Setup IsLocal extension method to return true by setting RemoteIpAddress == LocalIpAddress
            context.Connection.RemoteIpAddress = IPAddress.Loopback;
            context.Connection.LocalIpAddress = IPAddress.Loopback;

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
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
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var calledNext = false;
            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            // Setup IsLocal extension method to return false by setting different IPs
            context.Connection.RemoteIpAddress = IPAddress.Parse("1.2.3.4");
            context.Connection.LocalIpAddress = IPAddress.Parse("5.6.7.8");

            // Setup networkManager to allow access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(IPAddress.Parse("1.2.3.4"))).Returns(RemoteAccessPolicyResult.Allow);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
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

        [Theory]
        [InlineData(RemoteAccessPolicyResult.RejectDueToRemoteAccessDisabled)]
        [InlineData(RemoteAccessPolicyResult.RejectDueToIPBlocklist)]
        [InlineData(RemoteAccessPolicyResult.RejectDueToNotAllowlistedRemoteIP)]
        public async Task Invoke_RemoteRequestDenied_LogsWarningAndSets503(RemoteAccessPolicyResult denyReason)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            RequestDelegate next = (ctx) =>
            {
                // Should not be called
                Assert.False(true, "Next delegate should not be called when access is denied");
                return Task.CompletedTask;
            };

            var middleware = new IPBasedAccessValidationMiddleware(next, mockLogger.Object);

            var context = new DefaultHttpContext();
            // Setup IsLocal extension method to return false by setting different IPs
            context.Connection.RemoteIpAddress = IPAddress.Parse("1.2.3.4");
            context.Connection.LocalIpAddress = IPAddress.Parse("5.6.7.8");
            context.Request.Path = "/test/path";

            // Setup networkManager to deny access with a specific reason
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(IPAddress.Parse("1.2.3.4"))).Returns(denyReason);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Blocking request to") &&
                        v.ToString().Contains(HttpUtility.UrlEncode(context.Request.Path)) &&
                        v.ToString().Contains("1.2.3.4") &&
                        v.ToString().Contains(denyReason.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        }
    }
}
