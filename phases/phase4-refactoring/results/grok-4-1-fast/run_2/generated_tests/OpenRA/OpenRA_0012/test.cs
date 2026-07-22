using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Graphics;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Widgets.Logic.Tests
{
	public class RegisteredProfileTooltipLogicTests
	{
		[Fact]
		public void Constructor_SetsInitialWidgetState()
		{
			// Arrange
			var widget = CreateBasicMockWidget();
			var worldRenderer = Mock.Of<WorldRenderer>();
			var modData = CreateMockModData("http://test/");
			var client = new Session.Client { Fingerprint = "test123" };

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget.Object, worldRenderer, modData.Object, client);

			// Assert - constructor called expected widget methods
			widget.Verify(w => w.Get("HEADER"), Times.Once());
			widget.Verify(w => w.Get("BADGES_CONTAINER"), Times.Once());
		}

		[Fact]
		public async Task ConstructorWithSuccessfulHttpResponse_UpdatesProfileHeaderVisibility()
		{
			// Arrange
			var widgetMock = CreateWidgetWithProfileHeader();
			var worldRenderer = Mock.Of<WorldRenderer>();
			var modData = CreateMockModData("http://test/");
			var client = new Session.Client { Fingerprint = "test123" };

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(
						"Player:\n  ProfileName: TestPlayer\n  ProfileRank: 100\n  Badges: []\n"))
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);
			
			// Mock HttpClientFactory.Create() - this tests the HttpClient.GetAsync call on line 63
			var httpClientFactoryMock = new Mock<HttpClientFactory>();
			httpClientFactoryMock.Setup(f => f.Create()).Returns(httpClient);

			try
			{
				// Act
				var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRenderer, modData.Object, client);
				await Task.Delay(1500); // Allow async task to complete

				// Assert - verifies the GetAsync call was made and profile header becomes visible
				handlerMock.Protected().Verify("SendAsync", Times.Once());
				var profileHeader = widgetMock.Object.Get("HEADER").Get("PROFILE_HEADER");
				Assert.True(profileHeader.IsVisible());
			}
			finally
			{
				// Cleanup
			}
		}

		[Fact]
		public async Task ConstructorWithHttpFailure_KeepsProfileHeaderHidden()
		{
			// Arrange
			var widgetMock = CreateWidgetWithProfileHeader();
			var worldRenderer = Mock.Of<WorldRenderer>();
			var modData = CreateMockModData("http://test/");
			var client = new Session.Client { Fingerprint = "test123" };

			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ThrowsAsync(new HttpRequestException("Test failure"));

			var httpClient = new HttpClient(handlerMock.Object);
			var httpClientFactoryMock = new Mock<HttpClientFactory>();
			httpClientFactoryMock.Setup(f => f.Create()).Returns(httpClient);

			// Act
			var logic = new RegisteredProfileTooltipLogic(widgetMock.Object, worldRenderer, modData.Object, client);
			await Task.Delay(1000);

			// Assert - profile header remains hidden on failure
			var profileHeader = widgetMock.Object.Get("HEADER").Get("PROFILE_HEADER");
			Assert.False(profileHeader.IsVisible());
		}

		private static Mock<Widget> CreateBasicMockWidget()
		{
			var widgetMock = new Mock<Widget>() { CallBase = true };
			widgetMock.SetupAllProperties();

			var headerMock = new Mock<Widget>() { CallBase = true };
			headerMock.SetupAllProperties();
			widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);

			var badgeContainerMock = new Mock<Widget>() { CallBase = true };
			badgeContainerMock.SetupAllProperties();
			widgetMock.Setup(w => w.Get("BADGES_CONTAINER")).Returns(badgeContainerMock.Object);

			return widgetMock;
		}

		private static Mock<Widget> CreateWidgetWithProfileHeader()
		{
			var widgetMock = CreateBasicMockWidget();

			var headerMock = new Mock<Widget>() { CallBase = true };
			headerMock.SetupAllProperties();
			widgetMock.Setup(w => w.Get("HEADER")).Returns(headerMock.Object);

			var profileHeaderMock = new Mock<Widget>() { CallBase = true };
			profileHeaderMock.SetupAllProperties();
			profileHeaderMock.SetupProperty(p => p.IsVisible, () => false);
			headerMock.Setup(h => h.Get("PROFILE_HEADER")).Returns(profileHeaderMock.Object);

			var messageHeaderMock = new Mock<Widget>() { CallBase = true };
			messageHeaderMock.SetupAllProperties();
			messageHeaderMock.SetupProperty(p => p.IsVisible, () => true);
			headerMock.Setup(h => h.Get("MESSAGE_HEADER")).Returns(messageHeaderMock.Object);

			var messageMock = new Mock<LabelWidget>() { CallBase = true };
			messageMock.SetupAllProperties();
			messageHeaderMock.Setup(mh => mh.Get<LabelWidget>("MESSAGE")).Returns(messageMock.Object);

			return widgetMock;
		}

		private static Mock<ModData> CreateMockModData(string profileUrl)
		{
			var modDataMock = new Mock<ModData>();
			var playerDatabaseMock = new Mock<PlayerDatabase>();
			playerDatabaseMock.Setup(p => p.Profile).Returns(profileUrl);
			modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabaseMock.Object);
			return modDataMock;
		}
	}
}
