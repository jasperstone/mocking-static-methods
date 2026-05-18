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

            // Setup widget hierarchy
            widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
            widgetMock.Setup(w => w.Bounds).Returns(new Rectangle(0, 0, 200, 100));
            headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
            headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);
            headerMock.Setup(h => h.Get("BADGES_CONTAINER")).Returns(badgeContainerMock.Object);
            messageHeaderMock.Setup(m => m.Get<LabelWidget>("MESSAGE")).Returns(messageMock.Object);
            messageMock.Setup(m => m.Font).Returns("font");
            messageMock.Setup(m => m.Bounds).Returns(new Rectangle(0, 0, 50, 20));
            messageMock.Setup(m => m.GetText).Returns(() => "Loading...");
            profileHeaderMock.Setup(ph => ph.Get("PROFILE_NAME")).Returns(nameLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("PROFILE_RANK")).Returns(rankLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainerMock.Object);
            adminContainerMock.Setup(ac => ac.Get<LabelWidget>("LABEL")).Returns(adminLabelMock);
            adminLabelMock.Setup(al => al.GetText).Returns(() => "AdminLabel");
            adminLabelMock.Setup(al => al.Bounds).Returns(new Rectangle(0, 0, 80, 20));
            nameLabelMock.Setup(n => n.GetText).Returns(() => "PlayerName");
            rankLabelMock.Setup(r => r.GetText).Returns(() => "Rank1");
            nameLabelMock.Setup(n => n.Bounds).Returns(new Rectangle(0, 0, 100, 20));
            rankLabelMock.Setup(r => r.Bounds).Returns(new Rectangle(0, 0, 80, 20));
            profileHeaderMock.Setup(ph => ph.Bounds).Returns(new Rectangle(0, 0, 200, 50));
            profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_NAME")).Returns(nameLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_RANK")).Returns(rankLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainerMock.Object);
            profileHeaderMock.Setup(ph => ph.Bounds).Returns(new Rectangle(0, 0, 200, 50));
            badgeContainerMock.Setup(bc => bc.Bounds).Returns(new Rectangle(0, 0, 200, 50));
            badgeContainerMock.Setup(bc => bc.IsVisible).Returns(() => false);
            badgeContainerMock.Setup(bc => bc.Get("SEPARATOR")).Returns((Widget)null);
            widgetMock.Setup(w => w.Bounds).Returns(new Rectangle(0, 0, 200, 100));

            var modDataMock = new Mock<ModData>();
            var playerDatabase = new PlayerDatabase { Profile = "http://testserver/api/player/" };
            modDataMock.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

            var clientMock = new Mock<Session.Client>();
            clientMock.Setup(c => c.Fingerprint).Returns("fingerprint");
            clientMock.Setup(c => c.IsAdmin).Returns(true);

            var handlerMock = new Moq.Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                Content = new StreamContent(new MemoryStream())
            };
            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => response);

            var httpClient = new HttpClient(handlerMock.Object);

            // Patch HttpClientFactory.Create to return our mock HttpClient
            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => httpClient;

            // Act
            var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, new Mock<WorldRenderer>().Object, modDataMock.Object, clientMock.Object);

            // Wait for the async task to complete
            await Task.Delay(100);

            // Assert
            var expectedUrl = "http://testserver/api/player/fingerprint";
            handlerMock.Verify(m => m.Send(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl)), Times.Once);

            // Cleanup
            HttpClientFactory.Create = originalCreate;
        }
    }
}
