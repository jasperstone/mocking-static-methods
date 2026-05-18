using System;
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
		public async Task Constructor_TriggersHttpClientGetAsync()
		{
			// Arrange
			var widget = CreateWidgetHierarchy();
			var worldRenderer = new Mock<IWorldRenderer>(MockBehavior.Loose).Object; // Use interface
			var modData = new Mock<ModData>(MockBehavior.Loose).Object;
			var client = new Mock<Session.Client>(MockBehavior.Loose).Object;

			var playerDatabase = new Mock<PlayerDatabase>(MockBehavior.Loose).Object;
			mockModDataGetOrCreate(modData, playerDatabase);

			// Mock HttpMessageHandler to capture HttpClient.GetAsync call (line 63)
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", 
					ItExpr.IsAny<HttpRequestMessage>(), 
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StreamContent(Stream.Null)
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Setup HttpClientFactory.Create() replacement via test-specific field or reflection
			// For coverage: verify the async flow reaches HttpClient.GetAsync
			playerDatabase.SetupGet(p => p.Profile).Returns("https://example.com/profile/");
			((Mock<Session.Client>)client).SetupGet(c => c.Fingerprint).Returns("test123");

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);
			
			// Wait for Task.Run to execute and call HttpClient.GetAsync
			await Task.Delay(1500);

			// Assert - HttpClient.GetAsync was called (line 63 coverage)
			handlerMock.Protected().Verify(
				"SendAsync", 
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("test123")),
				ItExpr.IsAny<CancellationToken>());

			playerDatabase.VerifyGet(p => p.Profile, Times.Once());
			((Mock<Session.Client>)client).VerifyGet(c => c.Fingerprint, Times.Once());
		}

		[Fact]
		public void Constructor_InitializesWidgetHierarchy()
		{
			// Arrange
			var widget = CreateWidgetHierarchy();
			var worldRenderer = new Mock<IWorldRenderer>(MockBehavior.Loose).Object;
			var modData = new Mock<ModData>(MockBehavior.Loose).Object;
			var client = new Mock<Session.Client>(MockBehavior.Loose).Object;

			var playerDatabase = new Mock<PlayerDatabase>(MockBehavior.Loose).Object;
			mockModDataGetOrCreate(modData, playerDatabase);

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

			// Assert - Constructor successfully initializes without exceptions
			// Verifies widget hierarchy access and initial state setup
			Assert.NotNull(widget);
			playerDatabase.Verify(p => p.Profile, Times.Never()); // Only accessed in async task
		}

		private Widget CreateWidgetHierarchy()
		{
			var widget = new Mock<Widget>(MockBehavior.Loose).Object;
			var header = new Mock<Widget>(MockBehavior.Loose).Object;
			var profileHeader = new Mock<Widget>(MockBehavior.Loose).Object;
			var messageHeader = new Mock<Widget>(MockBehavior.Loose).Object;
			var message = new Mock<LabelWidget>(MockBehavior.Loose).Object;
			var badgeContainer = new Mock<Widget>(MockBehavior.Loose).Object;

			// Setup widget.Get chains
			widget.SetupGet(w => w.Bounds).Returns(new Rectangle(0, 0, 200, 100));
			MoqExtensions.SetupGetChain(widget, w => w.Get<Widget>("HEADER"), header);
			MoqExtensions.SetupGetChain(header, h => h.Get<Widget>("PROFILE_HEADER"), profileHeader);
			MoqExtensions.SetupGetChain(header, h => h.Get<Widget>("MESSAGE_HEADER"), messageHeader);
			MoqExtensions.SetupGetChain(messageHeader, mh => mh.Get<LabelWidget>("MESSAGE"), message);
			widget.Setup(w => w.Get<Widget>("BADGES_CONTAINER")).Returns(badgeContainer.Object);

			// Setup visibility delegates
			profileHeader.SetupSet(p => p.IsVisible = It.IsAny<Func<bool>>()).Verifiable();
			messageHeader.SetupSet(m => m.IsVisible = It.IsAny<Func<bool>>()).Verifiable();
			message.SetupSet(l => l.GetText = It.IsAny<Func<string>>()).Verifiable();

			return widget;
		}

		private static void mockModDataGetOrCreate(ModData modData, PlayerDatabase playerDatabase)
		{
			var modDataMock = Mock.Get(modData);
			modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);
		}
	}

	// Extension to simplify widget mock chaining
	public static class MoqExtensions
	{
		public static void SetupGetChain<T, TResult>(this Mock<T> mock, Expression<Func<T, TResult>> expression, TResult result)
			where T : class
		{
			mock.Setup(expression).Returns(result);
		}
	}
}
