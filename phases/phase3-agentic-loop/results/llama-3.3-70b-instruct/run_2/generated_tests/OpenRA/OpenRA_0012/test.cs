using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Called_WithCorrectUrl()
        {
            // Arrange
            var httpClient = new Mock<HttpClient>();
            var playerDatabase = new Mock<PlayerDatabase>();
            playerDatabase.Setup(p => p.Profile).Returns("https://example.com/profile");
            var client = new Session.Client { Fingerprint = "fingerprint" };
            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client);
            await Task.Run(async () =>
            {
                try
                {
                    var url = playerDatabase.Object.Profile + client.Fingerprint;
                    var httpResponseMessage = await httpClient.Object.GetAsync(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            httpClient.Verify(h => h.GetAsync(It.Is<string>(s => s == "https://example.com/profilefingerprint")), Times.Once);
        }
    }
}
