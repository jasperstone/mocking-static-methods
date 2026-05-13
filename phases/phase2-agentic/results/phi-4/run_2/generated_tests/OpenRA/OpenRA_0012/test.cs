using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_CallsHttpClientAndGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Player: { ProfileName: 'Test', ProfileRank: '1' }")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(response)
                .Verifiable();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<Func<HttpClient>>();
            mockHttpClientFactory.Setup(_ => _()).Returns(httpClient);

            var widget = new Widget(new WidgetArgs());
            var worldRenderer = new WorldRenderer();
            var modData = new ModData();
            var client = new Session.Client { Fingerprint = "test-fingerprint", IsAdmin = false };

            var playerDatabase = new PlayerDatabase
            {
                Profile = "http://example.com/profile/"
            };

            modData.SetOrCreate(playerDatabase);

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("http://example.com/profile/test-fingerprint")),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }
    }
}
