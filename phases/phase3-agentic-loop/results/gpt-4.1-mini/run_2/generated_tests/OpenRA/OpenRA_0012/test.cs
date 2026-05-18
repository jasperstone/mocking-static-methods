using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class RegisteredProfileTooltipLogicTests
	{
		// Helper to create a minimal Widget with nested widgets and bounds
		private Widget CreateWidgetWithStructure()
		{
			var widget = new Widget { Bounds = new Rectangle(0, 0, 200, 100) };

			var header = new Widget { Name = "HEADER", Bounds = new Rectangle(0, 0, 200, 50) };
			var badgesContainer = new Widget { Name = "BADGES_CONTAINER", Bounds = new Rectangle(0, 50, 200, 50) };
			var badgeSeparator = new Widget { Name = "SEPARATOR", Bounds = new Rectangle(5, 5, 190, 2) };

			badgesContainer.AddChild(badgeSeparator);

			widget.AddChild(header);
			widget.AddChild(badgesContainer);

			var profileHeader = new Widget { Name = "PROFILE_HEADER", Bounds = new Rectangle(0, 0, 200, 25) };
			var messageHeader = new Widget { Name = "MESSAGE_HEADER", Bounds = new Rectangle(0, 25, 200, 25) };

			header.AddChild(profileHeader);
			header.AddChild(messageHeader);

			var messageLabel = new LabelWidget { Name = "MESSAGE", Font = "default", Bounds = new Rectangle(5, 5, 190, 20) };
			messageHeader.AddChild(messageLabel);

			var profileNameLabel = new LabelWidget { Name = "PROFILE_NAME", Font = "default", Bounds = new Rectangle(5, 5, 190, 20) };
			var profileRankLabel = new LabelWidget { Name = "PROFILE_RANK", Font = "default", Bounds = new Rectangle(5, 25, 190, 20) };
			profileHeader.AddChild(profileNameLabel);
			profileHeader.AddChild(profileRankLabel);

			var gameAdmin = new Widget { Name = "GAME_ADMIN", Bounds = new Rectangle(0, 45, 200, 20) };
			var adminLabel = new LabelWidget { Name = "LABEL", Font = "default", Bounds = new Rectangle(5, 5, 190, 20) };
			gameAdmin.AddChild(adminLabel);
			profileHeader.AddChild(gameAdmin);

			return widget;
		}

		[Fact]
		public async Task RegisteredProfileTooltipLogic_CallsHttpClientGetAsync()
		{
			// Arrange
			var widget = CreateWidgetWithStructure();

			var modDataMock = new Mock<ModData>();
			var playerDatabase = new PlayerDatabase { Profile = "http://fakeprofile/" };
			modDataMock.Setup(m => m.GetOrCreate<PlayerDatabase>()).Returns(playerDatabase);

			var client = new Session.Client { Fingerprint = "12345", IsAdmin = false };

			// Setup HttpClient mock to intercept GetAsync call
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == playerDatabase.Profile + client.Fingerprint),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Player: { ProfileName: 'Test', ProfileRank: 'R1', Badges: [] }")))
				})
				.Verifiable();

			// Replace HttpClientFactory.Create to return our HttpClient with mocked handler
			HttpClientFactory.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));

			// Setup Game.Renderer.Fonts to return a dummy font that returns fixed size
			Game.Renderer = new Renderer();
			Game.Renderer.Fonts["default"] = new DummyFont();

			// Setup FluentProvider.GetMessage to return a fixed string
			FluentProvider.SetMessageProvider(key => key == "label-loading-player-profile" ? "Loading..." : "Failed");

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, null, modDataMock.Object, client);

			// Wait some time for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == playerDatabase.Profile + client.Fingerprint),
				ItExpr.IsAny<CancellationToken>());

			// Cleanup
			HttpClientFactory.ResetHttpClientFactory();
		}

		// Dummy implementations for dependencies

		private class DummyFont : IDisposable, IFont
		{
			public Vector2 Measure(string text) => new Vector2(text.Length * 5, 10);
			public void Dispose() { }
			public IGlyph CreateGlyph(char c, int size, float scale) => null;
		}

		private class Renderer
		{
			public System.Collections.Generic.Dictionary<string, IFont> Fonts { get; } = new System.Collections.Generic.Dictionary<string, IFont>();
		}

		private static class Game
		{
			public static Renderer Renderer { get; set; }

			public static void RunAfterTick(Action action)
			{
				// Run immediately for test
				action();
			}
		}

		private static class FluentProvider
		{
			private static Func<string, string> _messageProvider;

			public static void SetMessageProvider(Func<string, string> provider)
			{
				_messageProvider = provider;
			}

			public static string GetMessage(string key)
			{
				return _messageProvider?.Invoke(key) ?? key;
			}
		}

		private static class HttpClientFactory
		{
			private static Func<HttpClient> _factory;

			public static HttpClient Create()
			{
				return _factory != null ? _factory() : new HttpClient();
			}

			public static void SetHttpClientFactory(Func<HttpClient> factory)
			{
				_factory = factory;
			}

			public static void ResetHttpClientFactory()
			{
				_factory = null;
			}
		}
	}
}
