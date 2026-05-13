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
		// Helper to create a minimal Widget mock with necessary properties and methods
		private static Widget CreateWidgetMock(int width)
		{
			var widget = new Widget
			{
				Bounds = new Rectangle(0, 0, width, 10)
			};

			// Setup Get and GetOrNull to return dummy widgets for the test
			widget.AddChild("HEADER", new Widget { Bounds = new Rectangle(0, 0, width, 10) });
			widget.AddChild("BADGES_CONTAINER", new Widget { Bounds = new Rectangle(0, 0, width, 10) });
			widget.Get("HEADER").AddChild("PROFILE_HEADER", new Widget { Bounds = new Rectangle(0, 0, width, 10) });
			widget.Get("HEADER").AddChild("MESSAGE_HEADER", new Widget { Bounds = new Rectangle(0, 0, width, 10) });
			widget.Get("HEADER").Get("MESSAGE_HEADER").AddChild("MESSAGE", new LabelWidget { Font = "default", Bounds = new Rectangle(2, 0, 10, 10) });
			widget.Get("HEADER").Get("PROFILE_HEADER").AddChild("PROFILE_NAME", new LabelWidget { Font = "default", Bounds = new Rectangle(2, 0, 10, 10) });
			widget.Get("HEADER").Get("PROFILE_HEADER").AddChild("PROFILE_RANK", new LabelWidget { Font = "default", Bounds = new Rectangle(2, 0, 10, 10) });
			widget.Get("HEADER").Get("PROFILE_HEADER").AddChild("GAME_ADMIN", new Widget { Bounds = new Rectangle(0, 0, width, 10) });
			widget.Get("HEADER").Get("PROFILE_HEADER").Get("GAME_ADMIN").AddChild("LABEL", new LabelWidget { Font = "default", Bounds = new Rectangle(2, 0, 10, 10) });

			return widget;
		}

		[Fact]
		public async Task RegisteredProfileTooltipLogic_CallsHttpClientGetAsync()
		{
			// Arrange
			var widget = CreateWidgetMock(100);
			var worldRenderer = new WorldRenderer();
			var modData = new ModData();
			var client = new Session.Client { Fingerprint = "fingerprint", IsAdmin = false };

			// Setup PlayerDatabase with a Profile URL
			var playerDatabase = new PlayerDatabase { Profile = "http://testprofile/" };
			modData.SetOrCreate(playerDatabase);

			// Setup HttpClient mock to intercept GetAsync call
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StreamContent(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Player: {}")))
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Replace HttpClientFactory.Create to return our mock HttpClient
			HttpClientFactory.SetFactory(() => httpClient);

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, worldRenderer, modData, client);

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == playerDatabase.Profile + client.Fingerprint),
				ItExpr.IsAny<CancellationToken>());
		}
	}

	// Minimal stubs for dependencies to compile the test
	public class Widget
	{
		public Rectangle Bounds { get; set; } = new Rectangle();
		public bool Visible { get; set; }
		public Func<bool> IsVisible { get; set; }
		public bool VisibleSet { get; set; }
		private readonly System.Collections.Generic.Dictionary<string, Widget> children = new();

		public void AddChild(string name, Widget child) => children[name] = child;
		public Widget Get(string name) => children[name];
		public Widget GetOrNull(string name) => children.ContainsKey(name) ? children[name] : null;
	}

	public class LabelWidget : Widget
	{
		public string Font { get; set; }
		public Func<string> GetText { get; set; }
	}

	public struct Rectangle
	{
		public int X, Y, Width, Height;
		public Rectangle(int x = 0, int y = 0, int width = 0, int height = 0)
		{
			X = x; Y = y; Width = width; Height = height;
		}
	}

	public class WorldRenderer { }

	public class ModData
	{
		private PlayerDatabase playerDatabase;
		public T GetOrCreate<T>() where T : class, new()
		{
			if (typeof(T) == typeof(PlayerDatabase))
			{
				if (playerDatabase == null)
					playerDatabase = new PlayerDatabase();
				return playerDatabase as T;
			}
			return new T();
		}

		public void SetOrCreate(PlayerDatabase db)
		{
			playerDatabase = db;
		}
	}

	public class PlayerDatabase
	{
		public string Profile { get; set; }
	}

	public class Session
	{
		public class Client
		{
			public string Fingerprint { get; set; }
			public bool IsAdmin { get; set; }
		}
	}

	public static class HttpClientFactory
	{
		private static Func<HttpClient> factory = () => new HttpClient();
		public static HttpClient Create() => factory();
		public static void SetFactory(Func<HttpClient> newFactory) => factory = newFactory;
	}

	public static class FluentProvider
	{
		public static string GetMessage(string key) => key;
	}

	public static class Game
	{
		public static Renderer Renderer { get; } = new Renderer();
		public static void RunAfterTick(Action action) => action();
	}

	public class Renderer
	{
		public System.Collections.Generic.Dictionary<string, Font> Fonts { get; } = new()
		{
			{ "default", new Font() }
		};
	}

	public class Font
	{
		public Size Measure(string text) => new Size(text.Length * 5, 10);
	}

	public struct Size
	{
		public int X, Y;
		public Size(int x, int y) { X = x; Y = y; }
	}

	public static class MiniYaml
	{
		public static System.Collections.Generic.IEnumerable<(string Key, string Value)> FromStream(Stream stream, string url)
		{
			yield return ("Player", "{}");
		}
	}

	public static class FieldLoader
	{
		public static PlayerProfile Load<PlayerProfile>(string yamlValue) => new PlayerProfile();
	}

	public class PlayerProfile
	{
		public string ProfileName => "TestName";
		public string ProfileRank => "TestRank";
		public System.Collections.Generic.List<string> Badges { get; } = new();
	}

	public static class Ui
	{
		public static Widget LoadWidget(string name, Widget parent, WidgetArgs args) => new Widget { Bounds = new Rectangle(0, 0, 10, 10) };
	}

	public class WidgetArgs : System.Collections.Generic.Dictionary<string, object> { }

	public static class Log
	{
		public static void Write(string level, string message) { }
		public static void Write(string level, Exception e) { }
	}
}
