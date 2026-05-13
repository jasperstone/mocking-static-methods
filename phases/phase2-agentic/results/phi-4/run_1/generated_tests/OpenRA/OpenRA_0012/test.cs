using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Calls_GetAsyncOnHttpClient_WithCorrectUrl()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Player:\n  ProfileName: TestPlayer\n  ProfileRank: 1")
                });

            mockHttpClient.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockHttpMessageHandler.Object.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()));

            var mockPlayerDatabase = new Mock<PlayerDatabase>();
            mockPlayerDatabase.Setup(db => db.Profile).Returns("http://example.com/profile/");

            var mockClient = new Mock<Session.Client>();
            mockClient.Setup(c => c.Fingerprint).Returns("test-fingerprint");
            mockClient.Setup(c => c.IsAdmin).Returns(false);

            var mockWidget = new Mock<Widget>();
            var mockWorldRenderer = new Mock<WorldRenderer>();
            var mockModData = new Mock<ModData>();

            // Act
            var logic = new RegisteredProfileTooltipLogic(mockWidget.Object, mockWorldRenderer.Object, mockModData.Object, mockClient.Object)
            {
                HttpClient = mockHttpClient.Object,
                PlayerDatabase = mockPlayerDatabase.Object
            };

            await Task.Delay(100); // Wait for the async operation to complete

            // Assert
            mockHttpClient.Verify(client => client.GetAsync("http://example.com/profile/test-fingerprint"), Times.Once);
        }
    }
}
