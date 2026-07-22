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
        public async Task Invoke_Should_LogWarningAndSetStatusCode_When_AccessIsBlocked()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNext = new Mock<RequestDelegate>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var context = new DefaultHttpContext();

            // Setup HttpContext
            context.Request.Path = "/test/path";
            context.Response.Body = new System.IO.MemoryStream();

            // Setup HttpContext extension methods
            context.SetIsLocal(false);
            context.SetNormalizedRemoteIP("192.168.1.1");

            // Setup network manager to block access
            mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess("192.168.1.1"))
                .Returns(RemoteAccessPolicyResult.Blocked);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Blocking request to")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
        }
    }

    // Extension methods to set up HttpContext for testing
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

        public static void SetNormalizedRemoteIP(this HttpContext context, string ip)
        {
            context.Items["NormalizedRemoteIP"] = ip;
        }

        public static string GetNormalizedRemoteIP(this HttpContext context)
        {
            if (context.Items.TryGetValue("NormalizedRemoteIP", out var value) && value is string ip)
            {
                return ip;
            }
            return null;
        }
    }
}
