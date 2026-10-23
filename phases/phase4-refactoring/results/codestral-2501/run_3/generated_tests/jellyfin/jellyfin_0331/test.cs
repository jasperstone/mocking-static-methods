using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using System.Net;
using System.Web;
using Jellyfin.Api.Middleware;

public class IPBasedAccessValidationMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldLogWarning_WhenAccessIsBlocked()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        var mockNetworkManager = new Mock<INetworkManager>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();

        mockHttpContext.Setup(c => c.IsLocal()).Returns(false);
        mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(IPAddress.Parse("192.168.1.1"));
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);

        mockRequest.Setup(r => r.Path).Returns("/test");
        mockNetworkManager.Setup(nm => nm.ShouldAllowServerAccess(It.IsAny<IPAddress>())).Returns(RemoteAccessPolicyResult.Blocked);

        var middleware = new IPBasedAccessValidationMiddleware(
            next: (innerHttpContext) => Task.CompletedTask,
            logger: mockLogger.Object
        );

        // Act
        await middleware.Invoke(mockHttpContext.Object, mockNetworkManager.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.LogWarning(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, mockResponse.Object.StatusCode);
    }
}
