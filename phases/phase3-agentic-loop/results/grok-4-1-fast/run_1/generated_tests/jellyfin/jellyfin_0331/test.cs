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
    private readonly Mock<HttpRequest> _mockRequest;
    private readonly Mock<INetworkManager> _mockNetworkManager;
    private readonly IPBasedAccessValidationMiddleware _middleware;

    public IPBasedAccessValidationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        _mockRequest = new Mock<HttpRequest>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.SetupGet(c => c.Request).Returns(_mockRequest.Object);
        _mockNetworkManager = new Mock<INetworkManager>();

        _middleware = new IPBasedAccessValidationMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Invoke_LocalRequest_CallsNextAndReturns()
    {
        // Arrange
        _mockHttpContext.Setup(c => c.IsLocal()).Returns(true);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(n => n(_mockHttpContext.Object), Times.Once());
        _mockLogger.Verify(l => l.LogWarning(
            "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
            It.IsAny<object[]>()
        ), Times.Never);
    }

    [Fact]
    public async Task Invoke_NonLocalRequestAllowed_CallsNext()
    {
        // Arrange
        _mockHttpContext.Setup(c => c.IsLocal()).Returns(false);
        _mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(IPAddress.Parse("192.168.1.100"));
        _mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(It.IsAny<IPAddress>())).Returns(RemoteAccessPolicyResult.Allow);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(n => n(_mockHttpContext.Object), Times.Once());
        _mockLogger.Verify(l => l.LogWarning(
            "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
            It.IsAny<object[]>()
        ), Times.Never);
    }

    [Fact]
    public async Task Invoke_NonLocalRequestBlocked_LogsWarningAndSets503()
    {
        // Arrange
        var remoteIP = IPAddress.Parse("203.0.113.1");
        var path = "/api/test";
        var reason = RemoteAccessPolicyResult.LocalNetworkNotAllowed;

        _mockHttpContext.Setup(c => c.IsLocal()).Returns(false);
        _mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(remoteIP);
        _mockRequest.SetupGet(r => r.Path).Returns(path);
        _mockNetworkManager.Setup(n => n.ShouldAllowServerAccess(remoteIP)).Returns(reason);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(n => n(_mockHttpContext.Object), Times.Never);
        _mockHttpContext.VerifySet(c => c.Response.StatusCode = 503, Times.Once());

        _mockLogger.Verify(l => l.LogWarning(
            "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}",
            HttpUtility.UrlEncode(path),
            remoteIP,
            reason
        ), Times.Once);
    }
}
