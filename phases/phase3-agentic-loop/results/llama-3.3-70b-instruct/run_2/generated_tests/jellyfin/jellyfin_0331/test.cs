using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jellyfin.Api.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_LogsWarning_WhenAccessIsDenied()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            var responseMock = new Mock<HttpResponse>();

            httpContextMock.SetupGet(c => c.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

            var middleware = new IPBasedAccessValidationMiddleware(next: async context => { }, loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, null);

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>()), Times.Once);
        }
    }
}
