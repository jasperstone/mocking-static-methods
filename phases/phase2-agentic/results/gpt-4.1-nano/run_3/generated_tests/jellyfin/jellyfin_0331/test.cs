using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;

namespace Jellyfin.Tests.Api.Middleware
{
    public class IpBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LocalRequest_CallsNextAndDoesNotLogWarning()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/test/path";

            // Mock IsLocal to return true
            context.SetIsLocal(true);

            var mockNetworkManager = new Mock<INetworkManager>();

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Once);
            mockLogger.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task Invoke_NonLocalRequest_BlockedLogsWarningAndSetsStatus()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/blocked/path";

            // Mock IsLocal to return false
            context.SetIsLocal(false);
            context.SetGetNormalizedRemoteIP("192.168.1.100");
            context.Response.StatusCode = 200;

            var mockNetworkManager = new Mock<INetworkManager>();
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess("192.168.1.100"))
                .Returns(RemoteAccessPolicyResult.Block);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_NonLocalRequest_AllowedCallsNext()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/allowed/path";

            // Mock IsLocal to return false
            context.SetIsLocal(false);
            context.SetGetNormalizedRemoteIP("10.0.0.1");
            context.Response.StatusCode = 200;

            var mockNetworkManager = new Mock<INetworkManager>();
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess("10.0.0.1"))
                .Returns(RemoteAccessPolicyResult.Allow);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Once);
            mockLogger.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    // Extension methods to mock IsLocal and GetNormalizedRemoteIP
    public static class HttpContextExtensions
    {
        public static void SetIsLocal(this HttpContext context, bool isLocal)
        {
            context.Items["IsLocal"] = isLocal;
        }

        public static bool IsLocal(this HttpContext context)
        {
            if (context.Items.TryGetValue("IsLocal", out var value) && value is bool b)
            {
                return b;
            }
            return false;
        }

        public static void SetGetNormalizedRemoteIP(this HttpContext context, string ip)
        {
            context.Items["NormalizedRemoteIP"] = ip;
        }

        public static string GetNormalizedRemoteIP(this HttpContext context)
        {
            if (context.Items.TryGetValue("NormalizedRemoteIP", out var value) && value is string ip)
            {
                return ip;
            }
            return string.Empty;
        }
    }
}
