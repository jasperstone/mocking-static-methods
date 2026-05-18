using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using OpenRA.Network;
using OpenRA.Graphics;

namespace OpenRA.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_Called_With_Correct_Url()
        {
            // Arrange
            var widgetMock = new Mock<Widget>();
            var headerMock = new Mock<Widget>();
            var profileHeaderMock = new Mock<Widget>();
            var messageHeaderMock = new Mock<Widget>();
            var messageMock = new Mock<LabelWidget>();
            var badgeContainerMock = new Mock<Widget>();
            var adminContainerMock = new Mock<Widget>();
            var adminLabelMock = new Mock<LabelWidget>();
            var nameLabelMock = new Mock<LabelWidget>();
            var rankLabelMock = new Mock<LabelWidget>();
            var profileMock = new PlayerProfile { ProfileName = "TestName", ProfileRank = "TestRank" };
            var rendererMock = new Mock<Renderer>();
            var fontMock = new Mock<Font>();
            var clientMock = new Mock<Session.Client>();
            var modDataMock = new Mock<ModData>();
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            var httpClientMock = new Mock<HttpClient>();
            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var streamMock = new MemoryStream();

            // Setup mocks
            widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
            headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
            headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);
            messageHeaderMock.Setup(m => m.Get<LabelWidget>("MESSAGE")).Returns(messageMock.Object);
            messageMock.Setup(m => m.Font).Returns("DefaultFont");
            // Setup Game.Renderer.Fonts
            var fontsDict = new System.Collections.Generic.Dictionary<string, Font> { { "DefaultFont", fontMock.Object } };
            var gameMock = new Mock<Game>();
            gameMock.Setup(g => g.Renderer).Returns(rendererMock.Object);
            rendererMock.Setup(r => r.Fonts).Returns(fontsDict);
            // Setup message text
            var messageText = "Loading...";
            // Setup HttpClient
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.Create()).Returns(httpClientMock.Object);
            // Setup response
            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(streamMock);
            httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessageMock.Object);

            // Create the logic instance
            var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, rendererMock.Object, modDataMock.Object, clientMock.Object);

            // Act
            var url = "http://testprofile.com/" + "fingerprint123";
            var httpClient = HttpClientFactory.Create();
            var result = await httpClient.GetAsync(url);

            // Assert
            httpClientMock.Verify(c => c.GetAsync(It.Is<string>(s => s == url)), Times.Once);
        }
    }
}
