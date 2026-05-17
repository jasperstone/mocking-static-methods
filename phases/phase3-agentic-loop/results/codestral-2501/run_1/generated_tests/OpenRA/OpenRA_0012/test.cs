using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_ShouldBeCalled_WhenProfileIsLoaded()
        {
            // Arrange
            var widget = new Mock<Widget>();
            var worldRenderer = new Mock<WorldRenderer>();
            var modData = new Mock<ModData>();
            var client = new Mock<Session.Client>();
            var httpClient = new Mock<HttpClient>();

            var playerDatabase = new Mock<PlayerDatabase>();
            modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);

            var registeredProfileTooltipLogic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer.Object, modData.Object, client.Object);

            // Act
            await Task.Delay(1000); // Wait for the async task to complete

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
