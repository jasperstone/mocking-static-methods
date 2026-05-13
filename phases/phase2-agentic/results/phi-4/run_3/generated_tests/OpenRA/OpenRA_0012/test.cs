using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Calls_GetAsync_On_HttpClient()
        {
            // Arrange
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

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockPlayerDatabase = new Mock<PlayerDatabase>();
            mockPlayerDatabase.Setup(db => db.Profile).Returns("http://example.com/profile/");

            var mockClient = new Mock<Session.Client>();
            mockClient.Setup(c => c.Fingerprint).Returns("test-fingerprint");
            mockClient.Setup(c => c.IsAdmin).Returns(false);

            var mockWidget = new Mock<Widget>();
            var mockWorldRenderer = new Mock<WorldRenderer>();
            var mockModData = new Mock<ModData>();

            mockModData.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(mockPlayerDatabase.Object);

            var logic = new RegisteredProfileTooltipLogic(mockWidget.Object, mockWorldRenderer.Object, mockModData.Object, mockClient.Object);

            // Act
            await Task.Delay(100); // Allow the async task to complete

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
