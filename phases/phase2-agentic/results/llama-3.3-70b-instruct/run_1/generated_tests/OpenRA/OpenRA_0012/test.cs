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
        public async Task TestGetAsync()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var playerDatabase = new Mock<PlayerDatabase>();
            var client = new Session.Client();
            var widget = new Widget();

            httpClientFactory.Setup(f => f.Create()).Returns(httpClient.Object);
            httpClient.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMessage);

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget, new WorldRenderer(), new ModData(), client);
            await Task.Run(async () =>
            {
                try
                {
                    await logic.GetType().GetMethod("GetAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(logic, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            // Assert
            httpClient.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
