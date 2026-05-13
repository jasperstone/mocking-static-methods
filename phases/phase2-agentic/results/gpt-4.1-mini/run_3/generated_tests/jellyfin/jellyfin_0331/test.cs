using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using System.Net;
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
            context.Connection.LocalIpAddress = IPAddress.Loopback;

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
            // Setup IsLocal extension method to return false by setting different IPs
            context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.100");
            context.Connection.LocalIpAddress = IPAddress.Parse("192.168.1.1");
            // Setup Request.Path for logging
            context.Request.Path = "/testpath";

            // Setup GetNormalizedRemoteIP extension method to return the remote IP string
            // We simulate this by setting RemoteIpAddress and assuming the extension returns it as string
            // Setup networkManager to allow access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(RemoteAccessPolicyResult.Allow);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.True(calledNext);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_RemoteRequest_Denied_LogsWarningAndReturns503()
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
            context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
            context.Connection.LocalIpAddress = IPAddress.Parse("192.168.1.1");
            context.Request.Path = "/blockedpath";

            var remoteIpString = "10.0.0.1";
            var reason = RemoteAccessPolicyResult.Deny;

            // Setup networkManager to deny access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(reason);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.False(calledNext);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            // Verify LogWarning was called with expected parameters
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Blocking request to") &&
                        v.ToString().Contains(HttpUtility.UrlEncode(context.Request.Path)) &&
                        v.ToString().Contains(remoteIpString) &&
                        v.ToString().Contains(reason.ToString())
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy enum to match the RemoteAccessPolicyResult used in the middleware
    public enum RemoteAccessPolicyResult
    {
        Allow,
        Deny
    }

    // Dummy interface to match INetworkManager used in the middleware
    public interface INetworkManager
    {
        RemoteAccessPolicyResult ShouldAllowServerAccess(string remoteIp);
    }

    // Extension methods stubs to simulate the behavior used in middleware
    public static class HttpContextExtensions
    {
        public static bool IsLocal(this HttpContext context)
        {
            var connection = context.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (IPAddress.IsLoopback(connection.RemoteIpAddress))
                {
                    return true;
                }
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
            }
            return false;
        }

        public static string GetNormalizedRemoteIP(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
}
