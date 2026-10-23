using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Jellyfin.Api.Middleware;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Api.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarning_WhenAccessIsBlocked()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            networkManagerMock.Setup(n => n.ShouldAllowServerAccess(It.IsAny<string>())).Returns(RemoteAccessPolicyResult.Block);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.IsLocal()).Returns(false);
            httpContextMock.Setup(c => c.GetNormalizedRemoteIP()).Returns("192.168.1.100");
            httpContextMock.Setup(c => c.Request.Path).Returns("/path/to/resource");

            var middleware = new IPBasedAccessValidationMiddleware(next: async httpContext => { }, loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
