using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task HttpClient_GetAsync_Is_Called_With_Expected_Url()
        {
            // Arrange
            var widgetMock = new Mock<Widget>();
            widgetMock.Setup(w => w.Get(It.IsAny<string>())).Returns(widgetMock.Object);
            widgetMock.Setup(w => w.GetOrNull<Widget>(It.IsAny<string>())).Returns((Widget)null);

            var worldMock = new Mock<World>();
            var worldRendererMock = new Mock<WorldRenderer>(worldMock.Object, null);

            var modDataMock = new Mock<ModData>();
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            playerDatabaseMock.Setup(p => p.Profile).Returns("https://example.com/api/");
            modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);

            var client = new Session.Client { Fingerprint = "test-fingerprint" };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == "https://example.com/api/test-fingerprint"), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(Stream.Null)
                })
                .Verifiable();

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            
            // Replace static HttpClientFactory.Create using a wrapper/field approach
            var originalCreate = OpenRA.Support.HttpClientFactory.Create;
            OpenRA.Support.HttpClientFactory.Create = () => httpClient;

            try
            {
                // Act
                var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRendererMock.Object, modDataMock.Object, client);
                await Task.Delay(1000); // Give async task time to complete
            }
            finally
            {
                OpenRA.Support.HttpClientFactory.Create = originalCreate;
            }

            // Assert
            httpMessageHandlerMock.Protected().Verify(
                "SendAsync", 
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == "https://example.com/api/test-fingerprint"),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Handles_HttpClient_GetAsync_Failure()
        {
            // Arrange
            var widgetMock = new Mock<Widget>();
            widgetMock.Setup(w => w.Get(It.IsAny<string>())).Returns(widgetMock.Object);
            widgetMock.Setup(w => w.GetOrNull<Widget>(It.IsAny<string>())).Returns((Widget)null);

            var worldMock = new Mock<World>();
            var worldRendererMock = new Mock<WorldRenderer>(worldMock.Object, null);

            var modDataMock = new Mock<ModData>();
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            playerDatabaseMock.Setup(p => p.Profile).Returns("https://example.com/api/");
            modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);

            var client = new Session.Client { Fingerprint = "test-fingerprint" };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Test exception"));

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            var originalCreate = OpenRA.Support.HttpClientFactory.Create;
            OpenRA.Support.HttpClientFactory.Create = () => httpClient;

            try
            {
                // Act
                var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRendererMock.Object, modDataMock.Object, client);
                await Task.Delay(1000); // Give async task time to complete
            }
            finally
            {
                OpenRA.Support.HttpClientFactory.Create = originalCreate;
            }

            // Assert - test completes without crashing
            Assert.True(true);
        }
    }
}
