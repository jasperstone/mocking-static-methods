using Moq;
using Moq.Protected;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Calls_GetAsync_With_Correct_Url()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Player: { ProfileName: 'Test', ProfileRank: '1' }")
                });

            var playerDatabase = new Mock<PlayerDatabase>();
            playerDatabase.Setup(db => db.Profile).Returns("http://example.com/profile/");
            var client = new Session.Client { Fingerprint = "test-fingerprint", IsAdmin = false };

            var mockWidget = new Mock<Widget>();
            var mockWorldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();
            modData.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);

            // Act
            var logic = new RegisteredProfileTooltipLogic(mockWidget.Object, mockWorldRenderer.Object, modData.Object, client)
            {
                HttpClient = mockHttpClient.Object
            };

            // Start the logic to trigger the async operation
            var task = Task.Run(async () => await logic.InitializeAsync());

            // Wait for the task to complete
            await task;

            // Assert
            var expectedUrl = "http://example.com/profile/test-fingerprint";
            await mockHttpClient.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
