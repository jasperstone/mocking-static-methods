using System;
using System.IO;
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

            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();

            var clientMock = new Mock<Session.Client>();
            clientMock.Setup(c => c.Fingerprint).Returns("test-fingerprint");
            clientMock.Setup(c => c.IsAdmin).Returns(false);

            var playerDatabase = new PlayerDatabase
            {
                Profile = "http://example.com/profile/"
            };

            modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

            var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, clientMock.Object);

            // Act
            await Task.Delay(100); // Allow the async task to complete

            // Assert
            mockHttpClient.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString().Contains("http://example.com/profile/test-fingerprint")),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }
    }
}
