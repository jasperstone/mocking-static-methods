using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Graphics;

namespace OpenRA.Test
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
            var profileMock = new PlayerProfile { ProfileName = "TestName", ProfileRank = "TestRank", Badges = new System.Collections.Generic.List<string>() };
            var modDataMock = new Mock<ModData>();
            var clientMock = new Mock<Session.Client>();
            var gameRendererMock = new Mock<IGameRenderer>();
            var fontMock = new Mock<IFont>();
            var gameMock = new Mock<IGame>();
            var rendererMock = new Mock<IGameRenderer>();
            var gameInstanceMock = new Mock<IGame>();
            var gameRendererInstanceMock = new Mock<IGameRenderer>();
            var gameRendererFontsMock = new Mock<IFont>();
            var gameRendererFontsDict = new System.Collections.Generic.Dictionary<string, IFont> { { "font", fontMock.Object } };
            var gameMockInstance = new Mock<IGame>();
            var gameRendererMockInstance = new Mock<IGameRenderer>();
            var gameRendererFontsMockInstance = new Mock<IFont>();
            var gameRendererFontsDictInstance = new System.Collections.Generic.Dictionary<string, IFont> { { "font", fontMock.Object } };

            // Setup widget hierarchy
            widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
            headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
            headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);
            messageHeaderMock.Setup(m => m.Get<LabelWidget>("MESSAGE")).Returns(messageMock.Object);
            widgetMock.Setup(w => w.Bounds).Returns(new Rectangle(0, 0, 200, 100));
            messageMock.Setup(m => m.Font).Returns("font");
            // Setup Game.Renderer.Fonts
            var fontsDict = new System.Collections.Generic.Dictionary<string, IFont> { { "font", fontMock.Object } };
            var gameRendererMockObj = new Mock<IGameRenderer>();
            gameRendererMockObj.Setup(r => r.Fonts).Returns(fontsDict);
            // Setup Game static
            var gameStaticMock = new Mock<IGame>();
            gameStaticMock.Setup(g => g.Renderer).Returns(gameRendererMockObj.Object);
            // Setup PlayerDatabase
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            playerDatabaseMock.Setup(p => p.Profile).Returns("http://testprofile/");
            // Setup ModData
            modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);
            // Setup Session.Client
            clientMock.Setup(c => c.Fingerprint).Returns("fingerprint");
            // Setup HttpClientFactory
            var httpClientMock = new Mock<HttpClient>();
            var httpResponseMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var stream = new MemoryStream();
            var yamlString = "Player:\n  ProfileName: TestName\n  ProfileRank: TestRank\n  Badges: []";
            var yamlBytes = System.Text.Encoding.UTF8.GetBytes(yamlString);
            stream.Write(yamlBytes, 0, yamlBytes.Length);
            stream.Position = 0;
            contentMock.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(stream);
            httpResponseMock.Setup(r => r.Content).Returns(contentMock.Object);
            httpResponseMock.Setup(r => r.StatusCode).Returns(System.Net.HttpStatusCode.OK);
            var httpClientMockInstance = new Mock<HttpClient>();
            // Setup HttpClient.GetAsync to return our response
            httpClientMockInstance.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(httpResponseMock.Object);
            // Setup HttpClientFactory.Create to return our mock HttpClient
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.Create()).Returns(httpClientMockInstance.Object);
            // Inject the factory
            HttpClientFactory.SetFactory(httpClientFactoryMock.Object);

            // Act
            var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, gameRendererMockObj.Object, modDataMock.Object, clientMock.Object);
            // Use reflection to invoke the private GetAsync method
            var methodInfo = typeof(RegisteredProfileTooltipLogic).GetMethod("GetAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var url = "http://testprofile/" + "fingerprint";
            var task = (Task<HttpResponseMessage>)methodInfo.Invoke(logic, new object[] { url });
            var response = await task;
            Assert.NotNull(response);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }
}
