using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA;
using OpenRA.Graphics;
using OpenRA.Network;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Widgets.Logic.Tests
{
	public class RegisteredProfileTooltipLogicTests
	{
		private static readonly int WidgetWidth = 200;
		private static readonly int WidgetHeight = 100;

		[Fact]
		public async Task CallsHttpClientGetAsync_WithExpectedUrl()
		{
			// Arrange
			var widget = CreateMinimalMockWidget();
			var worldRenderer = new Mock<WorldRenderer>().Object;
			var modData = CreateMockModData("https://example.com/api/");
			var client = new Session.Client { Fingerprint = "test123" };

			var httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			httpMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

			var httpClient = new HttpClient(httpMessageHandler.Object);
			var originalCreate = OpenRA.Support.HttpClientFactory.Create;
			Action replaceFactory = () => OpenRA.Support.HttpClientFactory.Create = () => httpClient;

			try
			{
				replaceFactory();

				// Act
				var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
				await Task.Delay(300); // Wait for background Task.Run

				// Assert - verifies GetAsync was called on line 63 with correct URL
				httpMessageHandler.Protected().Verify(
					"SendAsync", Times.Once(),
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "https://example.com/api/test123"),
					ItExpr.IsAny<CancellationToken>());
			}
			finally
			{
				OpenRA.Support.HttpClientFactory.Create = originalCreate;
			}
		}

		[Fact]
		public async Task HandlesHttpClientFailure_WithoutCrashing()
		{
			// Arrange
			var widget = CreateMinimalMockWidget();
			var worldRenderer = new Mock<WorldRenderer>().Object;
			var modData = CreateMockModData("https://example.com/api/");
			var client = new Session.Client { Fingerprint = "test123" };

			var httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			httpMessageHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ThrowsAsync(new HttpRequestException("Test failure"));

			var httpClient = new HttpClient(httpMessageHandler.Object);
			var originalCreate = OpenRA.Support.HttpClientFactory.Create;
			Action replaceFactory = () => OpenRA.Support.HttpClientFactory.Create = () => httpClient;

			try
			{
				replaceFactory();

				// Act
				var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
				await Task.Delay(300);

				// Assert - call was attempted (GetAsync line 63 executed) and caught by try-catch
				httpMessageHandler.Protected().Verify(
					"SendAsync", Times.Once(),
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>());
			}
			finally
			{
				OpenRA.Support.HttpClientFactory.Create = originalCreate;
			}
		}

		private static Widget CreateMinimalMockWidget()
		{
			var widget = new Mock<Widget>(MockBehavior.Strict);
			widget.Setup(w => w.Get<Widget>("HEADER")).Returns(CreateMinimalHeaderWidget());
			widget.Setup(w => w.Get<Widget>("BADGES_CONTAINER")).Returns(new Mock<Widget>().Object);
			widget.SetupGet(w => w.Bounds).Returns(new Rectangle(0, 0, WidgetWidth, WidgetHeight));
			return widget.Object;
		}

		private static Widget CreateMinimalHeaderWidget()
		{
			var header = new Mock<Widget>(MockBehavior.Strict);
			var profileHeader = new Mock<Widget>();
			var messageHeader = new Mock<Widget>();
			
			header.Setup(h => h.Get<Widget>("PROFILE_HEADER")).Returns(profileHeader.Object);
			header.Setup(h => h.Get<Widget>("MESSAGE_HEADER")).Returns(messageHeader.Object);
			
			var message = new Mock<LabelWidget>();
			message.SetupGet(m => m.Font).Returns("TestFont");
			message.SetupGet(m => m.Bounds).Returns(new Rectangle(0, 0, 100, 20));
			messageHeader.Setup(h => h.Get<LabelWidget>("MESSAGE")).Returns(message.Object);
			
			profileHeader.SetupGet(p => p.Bounds).Returns(new Rectangle(0, 0, 100, 30));
			messageHeader.SetupGet(m => m.Bounds).Returns(new Rectangle(0, 0, 100, 20));
			
			header.SetupGet(h => h.Bounds).Returns(new Rectangle(0, 0, WidgetWidth, WidgetHeight));
			return header.Object;
		}

		private static ModData CreateMockModData(string profileUrl)
		{
			var modData = new Mock<ModData>(MockBehavior.Strict);
			var playerDatabase = new Mock<PlayerDatabase>();
			playerDatabase.SetupGet(p => p.Profile).Returns(profileUrl);
			modData.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase.Object);
			return modData.Object;
		}
	}
}
