using System.Net;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Api.Middleware.Tests
{
    public class IPBasedAccessValidationMiddlewareTests
    {
        [Fact]
        public async Task Invoke_ShouldLogWarning_WhenAccessIsDenied()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContextMock = new Mock<HttpContext>();
            var httpContextFeatureMock = new Mock<IHttpContextFeature>();
            var responseMock = new Mock<HttpResponse>();

            httpContextMock.Setup(ctx => ctx.Features).Returns(new FeatureCollection { httpContextFeatureMock.Object });
            httpContextMock.Setup(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextFeatureMock.Setup(feature => feature.Connection.RemoteIpAddress).Returns(IPAddress.Parse("192.168.1.1"));

            networkManagerMock.Setup(manager => manager.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Deny);

            var middleware = new IPBasedAccessValidationMiddleware(
                async context => await Task.CompletedTask,
                loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Blocking request to")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Invoke_ShouldNotLogWarning_WhenAccessIsAllowed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<IPBasedAccessValidationMiddleware>>();
            var networkManagerMock = new Mock<INetworkManager>();
            var httpContextMock = new Mock<HttpContext>();
            var httpContextFeatureMock = new Mock<IHttpContextFeature>();
            var responseMock = new Mock<HttpResponse>();

            httpContextMock.Setup(ctx => ctx.Features).Returns(new FeatureCollection { httpContextFeatureMock.Object });
            httpContextMock.Setup(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextFeatureMock.Setup(feature => feature.Connection.RemoteIpAddress).Returns(IPAddress.Parse("192.168.1.1"));

            networkManagerMock.Setup(manager => manager.ShouldAllowServerAccess(It.IsAny<IPAddress>()))
                .Returns(RemoteAccessPolicyResult.Allow);

            var middleware = new IPBasedAccessValidationMiddleware(
                async context => await Task.CompletedTask,
                loggerMock.Object);

            // Act
            await middleware.Invoke(httpContextMock.Object, networkManagerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
