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
		// Helper classes to mock dependencies minimally
		class DummyWidget : Widget
		{
			public DummyWidget() : base(null) { }
			public override Widget Get(string name) => this;
			public override Widget GetOrNull(string name) => null;
			public override T Get<T>(string name) => (T)(object)this;
			public override int BoundsLeft => 0;
			public override Rectangle Bounds { get; set; } = new Rectangle(0, 0, 100, 20);
			public override bool Visible { get; set; }
			public Func<string> GetText { get; set; }
			public Func<bool> IsVisible { get; set; }
			public string Font => "default";
		}

		class DummyLabelWidget : LabelWidget
		{
			public DummyLabelWidget() : base(null) { }
			public override int BoundsLeft => 0;
			public override Rectangle Bounds { get; set; } = new Rectangle(0, 0, 100, 20);
			public Func<string> GetText { get; set; }
			public override string Font => "default";
		}

		class DummyFont
		{
			public Vector2 Measure(string text) => new Vector2(text.Length * 5, 10);
		}

		class DummyRenderer
		{
			public System.Collections.Generic.Dictionary<string, DummyFont> Fonts { get; } = new()
			{
				["default"] = new DummyFont()
			};
		}

		class DummyGame
		{
			public DummyRenderer Renderer { get; } = new DummyRenderer();
			public void RunAfterTick(Action action) => action();
		}

		[Fact]
		public async Task Constructor_CallsHttpClientGetAsync()
		{
			// Arrange
			var widget = new DummyWidget();
			var worldRenderer = new object();
			var modData = new ModData();
			var client = new Session.Client { Fingerprint = "fingerprint", IsAdmin = false };

			// Setup PlayerDatabase with Profile URL
			var playerDatabase = new PlayerDatabase { Profile = "http://test/" };
			modData.Set(playerDatabase);

			// Setup HttpClient mock to intercept GetAsync call
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://test/fingerprint"),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Player: {}")
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Replace HttpClientFactory.Create to return our mock HttpClient
			HttpClientFactory.SetFactory(() => httpClient);

			// Setup Game static properties used in logic
			Game.Renderer = new DummyRenderer();
			Game.RunAfterTick = action => action();

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://test/fingerprint"),
				ItExpr.IsAny<CancellationToken>());
		}
	}
}
