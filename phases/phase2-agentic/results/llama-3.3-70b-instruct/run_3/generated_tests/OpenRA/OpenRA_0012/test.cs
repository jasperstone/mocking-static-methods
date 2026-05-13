using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task TestGetAsyncSuccess()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Player: { ProfileName: Test, ProfileRank: Test }")
            };
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.Create()).Returns(httpClient.Object);

            var playerDatabase = new Mock<PlayerDatabase>();
            playerDatabase.Setup(p => p.Profile).Returns("https://example.com/profile");

            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();
            modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);
            var client = new Mock<Session.Client>();
            client.Setup(c => c.Fingerprint).Returns("12345");

            var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

            // Act
            await Task.Run(async () =>
            {
                try
                {
                    await logic.GetType().GetField("profileLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(logic);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            Assert.True((bool)logic.GetType().GetField("profileLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(logic));
        }

        [Fact]
        public async Task TestGetAsyncFailure()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.Create()).Returns(httpClient.Object);

            var playerDatabase = new Mock<PlayerDatabase>();
            playerDatabase.Setup(p => p.Profile).Returns("https://example.com/profile");

            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();
            modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);
            var client = new Mock<Session.Client>();
            client.Setup(c => c.Fingerprint).Returns("12345");

            var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

            // Act
            await Task.Run(async () =>
            {
                try
                {
                    await logic.GetType().GetField("profileLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(logic);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            Assert.False((bool)logic.GetType().GetField("profileLoaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(logic));
        }
    }
}
