using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Api.Middleware.Tests;

public class IPBasedAccessValidationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<IPBasedAccessValidationMiddleware>> _mockLogger;
    private readonly Mock<INetworkManager> _mockNetworkManager;
    private readonly DefaultHttpContext _httpContext;

    public IPBasedAccessValidationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
        _mockNetworkManager = new Mock<INetworkManager>();

        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/test/path";
        _httpContext.Connection.RemoteIpAddress = new IPAddress(new byte[] { 192, 168, 1, 100 });
    }

    [Fact]
    public async Task Invoke_LocalRequest_CallsNextAndReturns()
    {
        // Arrange
        _httpContext.Connection.RemoteIpAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        var middleware = new IPBasedAccessValidationMiddleware(_mockNext.Object, _mockLogger.Object);

        // Act
        await middleware.Invoke(_httpContext, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(n => n(_httpContext), Times.Once());
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Invoke_NonLocalRequestAllowed_CallsNext()
    {
        // Arrange
        _mockNetworkManager.Setup(m => m.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
            .Returns(RemoteAccessPolicyResult.Allow);
        var middleware = new IPBasedAccessValidationMiddleware(_mockNext.Object, _mockLogger.Object);

        // Act
        await middleware.Invoke(_httpContext, _mockNetworkManager.Object);

        // Assert
        _mockNext.Verify(n => n(_httpContext), Times.Once());
        _mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Invoke_NonLocalRequestBlocked_LogsWarningAndSets503()
    {
        // Arrange
        var nonAllowResult = (RemoteAccessPolicyResult)1;
        _mockNetworkManager.Setup(m => m.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
            .Returns(nonAllowResult);
        var middleware = new IPBasedAccessValidationMiddleware(_mockNext.Object, _mockLogger.Object);

        // Act
        await middleware.Invoke(_httpContext, _mockNetworkManager.Object);

        // Assert
        _mockNext.VerifyNoOtherCalls();
        _mockLogger.Verify(
            x => x.LogWarning(
                It.Is<string>(msg => msg == "Blocking request to {Path} by {RemoteIP} due to IP filtering rule, reason: {Reason}"),
                It.Is<string>(path => path == HttpUtility.UrlEncode("/test/path")),
                It.IsAny<IPAddress>(),
                It.Is<RemoteAccessPolicyResult>(result => result == nonAllowResult)),
            Times.Once);
        Assert.Equal(503, _httpContext.Response.StatusCode);
    }
}
