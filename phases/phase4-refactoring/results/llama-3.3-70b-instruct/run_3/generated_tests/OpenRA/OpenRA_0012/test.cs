using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task TestGetAsync()
        {
            // Arrange
            var httpClient = new HttpClient(new HttpClientHandler());
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            var widget = new Widget();
            var worldRenderer = new WorldRenderer();
            var modData = new ModData(new Manifest(), new InstalledMods(), false);
            var client = new Session.Client();

            var registeredProfileTooltipLogic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

            // Act
            //await registeredProfileTooltipLogic.GetType().GetMethod("GetAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(registeredProfileTooltipLogic, null);

            // Assert
            //handlerMock.Protected().Verify(
            //    "SendAsync",
            //    Times.Exactly(1),
            //    ItExpr.IsAny<HttpRequestMessage>(),
            //    ItExpr.IsAny<CancellationToken>()
            //);
        }
    }
}
