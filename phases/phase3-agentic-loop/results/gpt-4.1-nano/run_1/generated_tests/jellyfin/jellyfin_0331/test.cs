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
            var wasNextCalled = false;
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns<HttpContext>(ctx =>
            {
                wasNextCalled = true;
                return Task.CompletedTask;
            });

            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Path = "/test/path";

            // Mock IsLocal to return true
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.IsLocal()).Returns(true);
            mockHttpContext.Setup(c => c.Request).Returns(context.Request);
            mockHttpContext.Setup(c => c.Response).Returns(context.Response);

            // Act
            await middleware.Invoke(context, Mock.Of<INetworkManager>());

            // Assert
            Assert.True(wasNextCalled);
            mockLogger.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[], object>()),
                Times.Never);
        }

        [Fact]
        public async Task Invoke_NonLocalBlockedLogsWarningAndSetsStatus()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();

            var context = new DefaultHttpContext();
            context.Request.Path = "/blocked/path";

            var remoteIp = "192.168.1.1";

            // Setup HttpContext extension methods
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.IsLocal()).Returns(false);
            mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(remoteIp);
            mockHttpContext.Setup(c => c.Request).Returns(context.Request);
            mockHttpContext.Setup(c => c.Response).Returns(context.Response);

            // Setup network manager to block access
            mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(remoteIp))
                .Returns(RemoteAccessPolicyResult.Block);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.Is<string>(s => s.Contains("Blocking request to")),
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_NonLocalAllowedCallsNextAndDoesNotLogWarning()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns<HttpContext>(ctx => Task.CompletedTask);
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();

            var context = new DefaultHttpContext();
            context.Request.Path = "/allowed/path";

            var remoteIp = "10.0.0.1";

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.IsLocal()).Returns(false);
            mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(remoteIp);
            mockHttpContext.Setup(c => c.Request).Returns(context.Request);
            mockHttpContext.Setup(c => c.Response).Returns(context.Response);

            mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(remoteIp))
                .Returns(RemoteAccessPolicyResult.Allow);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(context, mockNetworkManager.Object);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);
            mockNext.Verify(n => n(It.Is<HttpContext>(c => c == context)), Times.Once);
        }
    }
}
