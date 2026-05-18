using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Http; // Added for IHttpClientFactory
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using OpenRA.Network; // Added for Session
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_CallsHttpClientAndGetAsync()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StreamContent(new MemoryStream())
            };

            mockHttpClient
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(f => f.CreateClient("testClient")) // Provide a dummy name
                .Returns(mockHttpClient.Object);

            var playerDatabase = new Mock<PlayerDatabase>();
            playerDatabase
                .Setup(p => p.Profile)
                .Returns("http://example.com/profile/");

            var client = new Mock<Session.Client>();
            client
                .Setup(c => c.Fingerprint)
                .Returns("fingerprint");

            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();
            modData
                .Setup(m => m.GetOrCreate<PlayerDatabase>())
                .Returns(playerDatabase.Object);

            var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

            // Act
            await Task.Delay(100); // Allow the async task to run

            // Assert
            mockHttpClient.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("http://example.com/profile/fingerprint")),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }
    }
}
