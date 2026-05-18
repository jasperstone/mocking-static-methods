using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;

namespace Jellyfin.Api.Middleware.Tests;

public class IPBasedAccessValidationMiddlewareTests
{
    private readonly RequestDelegate _mockNext;
    private readonly Mock<ILogger<IPBasedAccessValidationMiddleware>> _mockLogger;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly IPBasedAccessValidationMiddleware _middleware;

    public IPBasedAccessValidationMiddlewareTests()
    {
        var nextTask = Task.CompletedTask;
        _mockNext = Mock.Of<RequestDelegate>(x => x(null!) == nextTask);

        _mockLogger = new();

        _mockHttpContext = new();
        var mockRequest = new Mock<HttpRequest>();
        var mockResponse = new Mock<HttpResponse>();
        _mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        _mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
        _mockHttpContext.Setup(c => c.IsLocal()).Returns(false);

        _middleware = new IPBasedAccessValidationMiddleware(_mockNext, _mockLogger.Object);
    }

    [Fact]
    public async Task Invoke_BlocksRemoteAccess_LogsWarning()
    {
        // Arrange
        var remoteIP = IPAddress.Parse("192.168.1.100");
        var path = "/api/test";
        
        _mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(remoteIP);
        _mockHttpContext.Setup(c => c.Request.Path).Returns(path);
        
        var networkManager = new BlockingNetworkManager(remoteIP);

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, networkManager);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Blocking request to") &&
                    v.ToString()!.Contains("%2Fapi%2Ftest") &&
                    v.ToString()!.Contains("192.168.1.100")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Assert.Equal(503, _mockHttpContext.Object.Response.StatusCode);
    }

    [Fact]
    public async Task Invoke_LocalRequest_AllowsAccess()
    {
        // Arrange
        _mockHttpContext.Setup(c => c.IsLocal()).Returns(true);
        var networkManager = new AllowingNetworkManager();

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, networkManager);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Invoke_AllowsRemoteAccess_ProceedsNormally()
    {
        // Arrange
        var remoteIP = IPAddress.Parse("192.168.1.1");
        _mockHttpContext.Setup(c => c.GetNormalizedRemoteIP()).Returns(remoteIP);
        var networkManager = new AllowingNetworkManager();

        // Act
        await _middleware.Invoke(_mockHttpContext.Object, networkManager);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private class BlockingNetworkManager : INetworkManager
    {
        private readonly IPAddress _blockIP;
        public BlockingNetworkManager(IPAddress blockIP) => _blockIP = blockIP;
        public RemoteAccessPolicyResult ShouldAllowServerAccess(IPAddress ip) => 
            _blockIP.Equals(ip) ? RemoteAccessPolicyResult.Block : RemoteAccessPolicyResult.Allow;
        public bool IsInLocalNetwork(IPAddress ip) => true;
        public bool IsInCustomNetwork(IPAddress ip, string customNetworks) => true;
    }

    private class AllowingNetworkManager : INetworkManager
    {
        public RemoteAccessPolicyResult ShouldAllowServerAccess(IPAddress ip) => RemoteAccessPolicyResult.Allow;
        public bool IsInLocalNetwork(IPAddress ip) => true;
        public bool IsInCustomNetwork(IPAddress ip, string customNetworks) => true;
    }
}
