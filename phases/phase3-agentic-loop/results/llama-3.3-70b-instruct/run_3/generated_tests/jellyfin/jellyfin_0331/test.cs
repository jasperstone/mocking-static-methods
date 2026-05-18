using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Jellyfin.Api.Middleware;

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
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            var responseMock = new Mock<HttpResponse>();

            httpContextMock.SetupGet(c => c.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

            requestMock.SetupGet(r => r.Path).Returns("/test");

            networkManagerMock.Setup(n => n.ShouldAllowServerAccess(It.IsAny<string>()))
                .Returns(RemoteAccessPolicyResult.Deny);

            var middleware = new IPBasedAccessValidationMiddleware(next: async context => { }, loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<RemoteAccessPolicyResult>()), Times.Once);
        }
    }
}
