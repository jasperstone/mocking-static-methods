using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Widgets.Logic.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task TestGetAsyncCall()
        {
            // Arrange
            var widget = new Widget("test");
            var worldRenderer = new WorldRenderer();
            var modData = new ModData(new Manifest("test", new Package()), new InstalledMods(new[] { "test" }, new[] { "test" }), false);
            var client = new Session.Client();

            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            var content = new StringContent("test content");
            httpResponseMessage.Content = content;

            httpClient.Setup(h => h.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(h => h.CreateClient()).Returns(httpClient.Object);

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
            await Task.Run(async () =>
            {
                try
                {
                    var url = "test url";
                    var response = await httpClient.Object.GetAsync(url);
                    var result = await response.Content.ReadAsStreamAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            httpClient.Verify(h => h.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
