using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Jellyfin.Api.Middleware;
using Jellyfin.Api;

namespace Jellyfin.Api.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarning_WhenAccessIsDenied()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            networkManagerMock.Setup(n => n.ShouldAllowServerAccess(It.IsAny<string>())).Returns(RemoteAccessPolicyResult.Deny);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.IsLocal()).Returns(false);
            httpContextMock.Setup(c => c.GetNormalizedRemoteIP()).Returns("192.168.1.100");
            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(r => r.Path).Returns("/path");
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            var responseMock = new Mock<HttpResponse>();
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            var middleware = new IPBasedAccessValidationMiddleware(next: async httpContext => { }, loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            responseMock.Verify(r => r.StatusCode = 503, Times.Once);
        }
    }
}
