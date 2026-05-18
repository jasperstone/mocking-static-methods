using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Net;
using Jellyfin.Api.Middleware;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        private Mock<HttpContext> CreateHttpContext(bool isLocal, string path)
        {
            var mockContext = new Mock<HttpContext>();
            
            var mockConnection = new Mock<Connection>();
            mockConnection.SetupGet(c => c.LocalIpAddress).Returns(isLocal ? IPAddress.Loopback : IPAddress.Loopback);
            mockConnection.SetupGet(c => c.RemoteIpAddress).Returns(isLocal ? IPAddress.Loopback : IPAddress.Parse("8.8.8.8"));
            mockContext.SetupGet(c => c.Connection).Returns(mockConnection.Object);
            
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(r => r.Path).Returns(new PathString(path));
            mockContext.SetupGet(c => c.Request).Returns(mockRequest.Object);

            var mockResponse = new Mock<HttpResponse>();
            mockContext.SetupGet(c => c.Response).Returns(mockResponse.Object);

            return mockContext;
        }

        [Fact]
        public async Task Invoke_LocalRequest_CallsNextMiddleware()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            
            var mockContext = CreateHttpContext(true, "/test");

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(mockContext.Object, mockNetworkManager.Object);

            // Assert
            mockNext.Verify(n => n(mockContext.Object), Times.Once);
            mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_NonLocalRequestAllowed_CallsNextMiddleware()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            mockNext.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var mockContext = CreateHttpContext(false, "/test");

            mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(It.IsAny<IPAddress>())).Returns(RemoteAccessPolicyResult.Allow);

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(mockContext.Object, mockNetworkManager.Object);

            // Assert
            mockNext.Verify(n => n(mockContext.Object), Times.Once);
            mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_NonLocalRequestBlocked_LogsWarning_Sets503_DoesNotCallNext()
        {
            // Arrange
            var mockNext = new Mock<RequestDelegate>();
            var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var mockNetworkManager = new Mock<INetworkManager>();
            var mockContext = CreateHttpContext(false, "/api/blocked?test=1");

            var remoteIP = IPAddress.Parse("8.8.8.8");
            mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(remoteIP)).Returns((RemoteAccessPolicyResult)1); // Any value != Allow (0)

            var middleware = new IPBasedAccessValidationMiddleware(mockNext.Object, mockLogger.Object);

            // Act
            await middleware.Invoke(mockContext.Object, mockNetworkManager.Object);

            // Assert
            ((Mock<HttpResponse>)mockContext.Object.Response).VerifySet(r => r.StatusCode = 503, Times.Once);
            mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Never);

            // Verify LogWarning called with correct template
            mockLogger.Verify(l => l.LogWarning(
                "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>()
            ), Times.Once);
        }
    }
}
