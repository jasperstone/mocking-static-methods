using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using MediaBrowser.Common.Net;

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
                l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[], object>()),
                Times.Never);
        }

        [Fact]
        public async Task Invoke_NonLocalRequest_BlockedLogsWarningAndSetsStatus()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();

            var context = new DefaultHttpContext();
            context.Request.Path = "/blocked/path";

            // Mock IsLocal to return false
            context.SetIsLocal(false);
            context.Response = new DefaultHttpResponse(context);

            var remoteIp = "192.168.1.100";

            // Mock GetNormalizedRemoteIP
            context.SetGetNormalizedRemoteIP(remoteIp);

            // Mock networkManager.ShouldAllowServerAccess to return not Allow
            mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(remoteIp))
                .Returns(RemoteAccessPolicyResult.Block);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockLogger.Verify(
                l => l.LogWarning(
                    It.Is<string>(s => s.Contains("Blocking request to")),
                    It.IsAny<object[]>()),
                Times.Once);
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
            if (context.Items.TryGetValue("NormalizedRemoteIP", out var value) && value is string s)
            {
                return s;
            }
            return string.Empty;
        }
    }
}
