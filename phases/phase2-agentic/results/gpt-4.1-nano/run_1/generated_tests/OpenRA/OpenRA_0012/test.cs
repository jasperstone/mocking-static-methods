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

namespace OpenRA.Tests
{
    public class RegisteredProfileTooltipLogicTests
    {
        [Fact]
        public async Task GetAsync_CallsHttpClientAndProcessesResponse()
        {
            // Arrange
            var widgetMock = new Mock<Widget>();
            var headerMock = new Mock<Widget>();
            var messageHeaderMock = new Mock<Widget>();
            var messageMock = new Mock<LabelWidget>();
            var profileHeaderMock = new Mock<Widget>();
            var badgeContainerMock = new Mock<Widget>();
            var adminContainerMock = new Mock<Widget>();
            var adminLabelMock = new Mock<LabelWidget>();
            var nameLabelMock = new Mock<LabelWidget>();
            var rankLabelMock = new Mock<LabelWidget>();
            var profileMock = new PlayerProfile { ProfileName = "TestName", ProfileRank = "TestRank", Badges = new System.Collections.Generic.List<string>() };
            var streamMock = new MemoryStream();

            // Setup widget hierarchy
            widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
            headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
            headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);
            messageHeaderMock.Setup(m => m.Get<LabelWidget>("MESSAGE")).Returns(messageMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("PROFILE_NAME")).Returns(nameLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("PROFILE_RANK")).Returns(rankLabelMock.Object);
            profileHeaderMock.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainerMock.Object);
            adminContainerMock.Setup(ac => ac.Get<LabelWidget>("LABEL")).Returns(adminLabelMock.Object);
            adminLabelMock.Setup(al => al.GetText).Returns(() => "Admin");
            nameLabelMock.Setup(n => n.GetText).Returns(() => "Name");
            rankLabelMock.Setup(r => r.GetText).Returns(() => "Rank");
            widgetMock.Setup(w => w.Bounds).Returns(new { Width = 200, Height = 50 });
            headerMock.Setup(h => h.Bounds).Returns(new { Width = 200, Height = 50 });
            messageHeaderMock.Setup(m => m.Bounds).Returns(new { Width = 200, Height = 20 });
            profileHeaderMock.Setup(ph => ph.Bounds).Returns(new { Width = 200, Height = 50 });
            badgeContainerMock.Setup(b => b.Bounds).Returns(new { Width = 200, Height = 0, Y = 0 });
            badgeContainerMock.Setup(b => b.GetOrNull("SEPARATOR")).Returns((Widget)null);
            badgeContainerMock.Setup(b => b.Bounds).Returns(new { Width = 200, Height = 0, Y = 0 });
            badgeContainerMock.Setup(b => b.IsVisible).Returns(() => false);
            widgetMock.Setup(w => w.Bounds).Returns(new { Width = 200, Height = 50 });
            widgetMock.Setup(w => w.Get("BADGES_CONTAINER")).Returns(badgeContainerMock.Object);
            widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
            widgetMock.Setup(w => w.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);

            // Mock HttpClientFactory to return a mock HttpClient
            var httpClientMock = new Mock<HttpClient>();
            var responseMessageMock = new Mock<HttpResponseMessage>();
            var contentMock = new Mock<HttpContent>();
            var yamlContent = new StringReader("Player: { name: 'TestName', rank: 'TestRank' }");
            var streamContent = new StreamReader(yamlContent).BaseStream;

            responseMessageMock.Setup(r => r.Content).Returns(contentMock.Object);
            contentMock.Setup(c => c.ReadAsStreamAsync()).ReturnsAsync(streamContent);
            httpClientMock.Setup(c => c.GetAsync(It.IsAny<string>())).ReturnsAsync(responseMessageMock.Object);

            // Patch HttpClientFactory.Create to return our mock HttpClient
            HttpClientFactory.Create = () => httpClientMock.Object;

            var widget = widgetMock.Object;
            var worldRenderer = new Mock<WorldRenderer>().Object;
            var modDataMock = new Mock<ModData>();
            var playerDatabaseMock = new Mock<PlayerDatabase>();
            modDataMock.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);
            var clientMock = new Mock<Session.Client>();
            clientMock.Setup(c => c.Fingerprint).Returns("fingerprint");
            clientMock.Setup(c => c.IsAdmin).Returns(true);

            // Act
            var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modDataMock.Object, clientMock.Object);

            // Wait for async task to complete
            await Task.Delay(100);

            // Assert
            httpClientMock.Verify(c => c.GetAsync(It.Is<string>(url => url.Contains("fingerprint"))), Times.Once);
            Assert.True(logic != null);
        }
    }
}
