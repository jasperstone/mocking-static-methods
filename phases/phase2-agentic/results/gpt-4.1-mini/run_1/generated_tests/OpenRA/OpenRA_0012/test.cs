using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class RegisteredProfileTooltipLogicTests
	{
		[Fact]
		public async Task Constructor_CallsHttpClientGetAsync_WithExpectedUrl()
		{
			// Arrange
			var widgetMock = new Mock<Widget>(MockBehavior.Strict);
			var headerMock = new Mock<Widget>(MockBehavior.Strict);
			var badgeContainerMock = new Mock<Widget>(MockBehavior.Strict);
			var badgeSeparatorMock = new Mock<Widget>(MockBehavior.Strict);
			var profileHeaderMock = new Mock<Widget>(MockBehavior.Strict);
			var messageHeaderMock = new Mock<Widget>(MockBehavior.Strict);
			var messageLabelMock = new Mock<LabelWidget>(MockBehavior.Strict);
			var fontMock = new Mock<Font>(MockBehavior.Strict);
			var worldRenderer = new WorldRenderer();
			var modDataMock = new Mock<ModData>(MockBehavior.Strict);
			var clientMock = new Mock<Session.Client>(MockBehavior.Strict);
			var playerDatabaseMock = new Mock<PlayerDatabase>(MockBehavior.Strict);

			// Setup widget hierarchy and returns
			widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);
			widgetMock.Setup(w => w.Get("BADGES_CONTAINER")).Returns(badgeContainerMock.Object);
			badgeContainerMock.Setup(bc => bc.GetOrNull("SEPARATOR")).Returns((Widget)null);
			headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);
			headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);
			messageHeaderMock.Setup(mh => mh.Get<LabelWidget>("MESSAGE")).Returns(messageLabelMock.Object);

			// Setup Bounds for widget and children
			widgetMock.SetupGet(w => w.Bounds).Returns(new Rectangle(0, 0, 100, 100));
			headerMock.SetupGet(h => h.Bounds).Returns(new Rectangle(0, 0, 100, 20));
			badgeContainerMock.SetupGet(bc => bc.Bounds).Returns(new Rectangle(0, 20, 100, 10));
			messageLabelMock.SetupGet(m => m.Font).Returns("default");
			messageLabelMock.SetupGet(m => m.Bounds).Returns(new Rectangle(2, 2, 50, 10));

			// Setup Font measurement
			Game.Renderer.Fonts["default"] = fontMock.Object;
			fontMock.Setup(f => f.Measure(It.IsAny<string>())).Returns(new Vector2(50, 10));

			// Setup FluentProvider to return a message string
			FluentProvider.SetMessage("label-loading-player-profile", "Loading...");
			FluentProvider.SetMessage("label-loading-player-profile-failed", "Failed to load");

			// Setup ModData to return PlayerDatabase
			modDataMock.Setup(md => md.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);

			// Setup PlayerDatabase Profile URL
			playerDatabaseMock.SetupGet(pd => pd.Profile).Returns("http://profile.url/");

			// Setup client fingerprint and admin flag
			clientMock.SetupGet(c => c.Fingerprint).Returns("fingerprint123");
			clientMock.SetupGet(c => c.IsAdmin).Returns(false);

			// Setup HttpClient mock with handler to intercept GetAsync call
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Get &&
						req.RequestUri == new Uri("http://profile.url/fingerprint123")),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Player: { ProfileName: 'Test', ProfileRank: 'Rank1' }")))
				})
				.Verifiable();

			// Replace HttpClientFactory.Create to return HttpClient with mocked handler
			HttpClientFactory.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));

			// Setup MiniYaml.FromStream to return a dummy yaml key-value pair
			MiniYaml.SetFromStreamFunc((stream, url) =>
			{
				return new[] { new KeyValuePair<string, string>("Player", "dummy yaml value") };
			});

			// Setup FieldLoader.Load to return a dummy PlayerProfile
			FieldLoader.SetLoadFunc<PlayerProfile>((yamlValue) =>
			{
				return new PlayerProfile { ProfileName = "Test", ProfileRank = "Rank1" };
			});

			// Setup Game.RunAfterTick to immediately run the action
			Game.SetRunAfterTickAction(action => action());

			// Setup profileHeader to return label widgets for PROFILE_NAME and PROFILE_RANK
			var nameLabelMock = new Mock<LabelWidget>(MockBehavior.Strict);
			var rankLabelMock = new Mock<LabelWidget>(MockBehavior.Strict);
			profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_NAME")).Returns(nameLabelMock.Object);
			profileHeaderMock.Setup(ph => ph.Get<LabelWidget>("PROFILE_RANK")).Returns(rankLabelMock.Object);

			// Setup fonts for name and rank labels
			Game.Renderer.Fonts["nameFont"] = fontMock.Object;
			Game.Renderer.Fonts["rankFont"] = fontMock.Object;
			nameLabelMock.SetupGet(nl => nl.Font).Returns("nameFont");
			rankLabelMock.SetupGet(rl => rl.Font).Returns("rankFont");
			nameLabelMock.SetupGet(nl => nl.Bounds).Returns(new Rectangle(2, 2, 50, 10));
			rankLabelMock.SetupGet(rl => rl.Bounds).Returns(new Rectangle(2, 2, 50, 10));
			nameLabelMock.SetupProperty(nl => nl.GetText);
			rankLabelMock.SetupProperty(rl => rl.GetText);

			// Setup profileHeader bounds
			profileHeaderMock.SetupGet(ph => ph.Bounds).Returns(new Rectangle(0, 0, 100, 20));
			messageHeaderMock.SetupGet(mh => mh.Bounds).Returns(new Rectangle(0, 0, 100, 10));

			// Setup badgeContainer and adminContainer for completeness
			var adminContainerMock = new Mock<Widget>(MockBehavior.Strict);
			var adminLabelMock = new Mock<LabelWidget>(MockBehavior.Strict);
			profileHeaderMock.Setup(ph => ph.Get("GAME_ADMIN")).Returns(adminContainerMock.Object);
			adminContainerMock.Setup(ac => ac.Get<LabelWidget>("LABEL")).Returns(adminLabelMock.Object);
			Game.Renderer.Fonts["adminFont"] = fontMock.Object;
			adminLabelMock.SetupGet(al => al.Font).Returns("adminFont");
			adminLabelMock.SetupGet(al => al.Bounds).Returns(new Rectangle(2, 2, 50, 10));
			adminLabelMock.SetupProperty(al => al.GetText, () => "Admin");
			adminContainerMock.SetupProperty(ac => ac.IsVisible);
			profileHeaderMock.SetupProperty(ph => ph.Bounds);

			// Act
			// Constructing the RegisteredProfileTooltipLogic triggers the async Task.Run
			var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRenderer, modDataMock.Object, clientMock.Object);

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri("http://profile.url/fingerprint123")),
				ItExpr.IsAny<CancellationToken>());
		}
	}
}
