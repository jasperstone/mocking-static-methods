using System.Net;
using System.Threading.Tasks;
using System.Web;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Api.Middleware.Tests;

public class IPBasedAccessValidationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<IPBasedAccessValidationMiddleware>> _mockLogger;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<INetworkManager> _mockNetworkManager;
    private readonly IPBasedAccessValidationMiddleware _middleware;

    public IPBasedAccessValidationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockNetworkManager = new Mock<INetworkManager>();

        SetupHttpContext();

        _middleware = new IPBasedAccessValidationMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    private void SetupHttpContext(bool isLocal = false, IPAddress remoteIP = null)
    {
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();

        mockRequest.Setup(x => x.Path).Returns("/test/path");

        mockResponse.SetupProperty(x => x.StatusCode);

        _mockHttpContext.Setup(x => x.IsLocal()).Returns(isLocal);
        _mockHttpContext.Setup(x => x.GetNormalizedRemoteIP()).Returns(remoteIP ?? IPAddress.Parse("192.168.1.100"));
        _mockHttpContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
        _mockHttpContext.SetupGet(x => x.Response).Returns(mockResponse.Object);
    }

    [Fact]
    public async Task Invoke_LocalRequest_CallsNextMiddleware()
    {
        // Arrange
        SetupHttpContext(isLocal: true);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once());
        _mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        _mockHttpContext.VerifySet(x => x.Response.StatusCode = It.IsAny<int>(), Times.Never);
    }

    [Fact]
    public async Task Invoke_NonLocalRequestWithAccessAllowed_CallsNextMiddleware()
    {
        // Arrange
        SetupHttpContext(isLocal: false);
        _mockNetworkManager.Setup(x => x.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
            .Returns(RemoteAccessPolicyResult.Allow);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Once());
        _mockLogger.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task Invoke_NonLocalRequestWithAccessDenied_LogsWarningAndReturns503()
    {
        // Arrange
        SetupHttpContext(isLocal: false);
        var denyReason = RemoteAccessPolicyResult.Block;
        var remoteIP = IPAddress.Parse("192.168.1.100");
        var encodedPath = HttpUtility.UrlEncode("/test/path");
        _mockNetworkManager.Setup(x => x.ShouldAllowServerAccess(remoteIP))
            .Returns(denyReason);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(x => x(_mockHttpContext.Object), Times.Never);
        _mockHttpContext.VerifySet(x => x.Response.StatusCode = 503, Times.Once());

        _mockLogger.Verify(
            x => x.LogWarning(
                "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
                encodedPath,
                remoteIP.ToString(),
                denyReason),
            Times.Once);
    }
}
