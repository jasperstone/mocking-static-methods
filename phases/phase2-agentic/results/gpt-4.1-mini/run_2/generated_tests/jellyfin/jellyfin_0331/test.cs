using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Api.Middleware;
using System.Net;
using System.Web;

namespace Jellyfin.Api.Tests.Middleware
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        private class TestHttpContext : DefaultHttpContext
        {
            public override bool IsLocal()
            {
                return _isLocal;
            }

            public override IPAddress GetNormalizedRemoteIP()
            {
                return _remoteIP;
            }

            private readonly bool _isLocal;
            private readonly IPAddress _remoteIP;

            public TestHttpContext(bool isLocal, IPAddress remoteIP)
            {
                _isLocal = isLocal;
                _remoteIP = remoteIP;
            }
        }

        private enum RemoteAccessPolicyResult
        {
            Allow,
            Deny,
            // Other possible values if needed
        }

        private interface INetworkManager
        {
            RemoteAccessPolicyResult ShouldAllowServerAccess(IPAddress ip);
        }

        [Fact]
        public async Task Invoke_LocalRequest_CallsNextDelegate()
        {
            // Arrange
            var calledNext = false;
            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var context = new TestHttpContext(isLocal: true, remoteIP: IPAddress.Loopback);

            var networkManagerMock = new Mock<INetworkManager>();

            // Act
            await middleware.Invoke(context, networkManagerMock.Object);

            // Assert
            Assert.True(calledNext);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_RemoteRequest_Allowed_CallsNextDelegate()
        {
            // Arrange
            var calledNext = false;
            RequestDelegate next = (ctx) =>
            {
                calledNext = true;
                return Task.CompletedTask;
            };

            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var remoteIp = IPAddress.Parse("192.168.1.1");
            var context = new TestHttpContext(isLocal: false, remoteIP: remoteIp);

            var networkManagerMock = new Mock<INetworkManager>();
            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess(remoteIp))
                .Returns(RemoteAccessPolicyResult.Allow);

            // Act
            await middleware.Invoke(context, networkManagerMock.Object);

            // Assert
            Assert.True(calledNext);
            Assert.NotEqual(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Invoke_RemoteRequest_Denied_LogsWarningAndSets503()
        {
            // Arrange
            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var middleware = new IPBasedAccessValidationMiddleware(next, loggerMock.Object);

            var remoteIp = IPAddress.Parse("10.0.0.1");
            var context = new TestHttpContext(isLocal: false, remoteIP: remoteIp);
            context.Request.Path = "/test/path";

            var networkManagerMock = new Mock<INetworkManager>();
            networkManagerMock.Setup(nm => nm.ShouldAllowServerAccess(remoteIp))
                .Returns(RemoteAccessPolicyResult.Deny);

            // Act
            await middleware.Invoke(context, networkManagerMock.Object);

            // Assert
            Assert.False(nextCalled);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);

            // Verify the LogWarning call with expected message and parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Blocking request to") &&
                        v.ToString().Contains(HttpUtility.UrlEncode(context.Request.Path)) &&
                        v.ToString().Contains(remoteIp.ToString()) &&
                        v.ToString().Contains("Deny")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
