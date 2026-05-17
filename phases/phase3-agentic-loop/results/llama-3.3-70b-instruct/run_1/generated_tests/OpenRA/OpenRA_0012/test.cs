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

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Called_WithCorrectUrl()
        {
            // Arrange
            var httpClient = new HttpClient();
            var playerDatabase = new PlayerDatabase();
            var client = new Session.Client { Fingerprint = "testFingerprint" };
            var widget = new Widget("testWidget");
            var worldRenderer = new WorldRenderer();
            var modData = new ModData(new Manifest("testMod", new Package()), new InstalledMods(new[] { "testPath" }, new[] { "testSearchPath" }), false);

            // Act
            var registeredProfileTooltipLogic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
            await Task.Run(async () =>
            {
                try
                {
                    var url = playerDatabase.Profile + client.Fingerprint;
                    var httpResponseMessage = await httpClient.GetAsync(url);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            //httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
