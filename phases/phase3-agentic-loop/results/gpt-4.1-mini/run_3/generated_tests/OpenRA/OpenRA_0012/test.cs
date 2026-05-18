using System;
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
		// Helper to create a minimal Widget mock with nested Get calls
		private class TestWidget : Widget
		{
			public TestWidget()
			{
				Bounds = new Rectangle(0, 0, 100, 20);
			}

			public override Widget Get(string name)
			{
				if (name == "HEADER")
					return new HeaderWidget();
				if (name == "BADGES_CONTAINER")
					return new BadgeContainerWidget();
				return base.Get(name);
			}

			public override Widget GetOrNull(string name)
			{
				if (name == "SEPARATOR")
					return new SeparatorWidget();
				return base.GetOrNull(name);
			}

			private class HeaderWidget : Widget
			{
				public HeaderWidget()
				{
					Bounds = new Rectangle(0, 0, 100, 20);
				}

				public override Widget Get(string name)
				{
					if (name == "PROFILE_HEADER")
						return new ProfileHeaderWidget();
					if (name == "MESSAGE_HEADER")
						return new MessageHeaderWidget();
					return base.Get(name);
				}
			}

			private class BadgeContainerWidget : Widget
			{
				public BadgeContainerWidget()
				{
					Bounds = new Rectangle(0, 0, 100, 20);
					Visible = false;
				}
			}

			private class SeparatorWidget : Widget
			{
				public SeparatorWidget()
				{
					Bounds = new Rectangle(1, 1, 10, 5);
				}
			}

			private class ProfileHeaderWidget : Widget
			{
				public ProfileHeaderWidget()
				{
					Bounds = new Rectangle(0, 0, 100, 20);
				}

				public override Widget Get(string name)
				{
					if (name == "PROFILE_NAME")
						return new LabelWidget("Arial", 5);
					if (name == "PROFILE_RANK")
						return new LabelWidget("Arial", 5);
					if (name == "GAME_ADMIN")
						return new AdminContainerWidget();
					return base.Get(name);
				}
			}

			private class AdminContainerWidget : Widget
			{
				public AdminContainerWidget()
				{
					Bounds = new Rectangle(0, 0, 100, 20);
				}

				public override Widget Get(string name)
				{
					if (name == "LABEL")
						return new LabelWidget("Arial", 5);
					return base.Get(name);
				}
			}

			private class MessageHeaderWidget : Widget
			{
				public MessageHeaderWidget()
				{
					Bounds = new Rectangle(0, 0, 100, 20);
				}

				public override Widget Get(string name)
				{
					if (name == "MESSAGE")
						return new LabelWidget("Arial", 5);
					return base.Get(name);
				}
			}

			private class LabelWidget : Widget
			{
				public string Font { get; }
				public int Left { get; }
				public Func<string> GetText { get; set; }

				public LabelWidget(string font, int left)
				{
					Font = font;
					Left = left;
					Bounds = new Rectangle(left, 0, 50, 10);
					GetText = () => "";
				}
			}
		}

		// Minimal Rectangle struct for Bounds
		private struct Rectangle
		{
			public int X, Y, Width, Height;
			public Rectangle(int x, int y, int width, int height)
			{
				X = x; Y = y; Width = width; Height = height;
			}
		}

		// Minimal FontMeasure mock
		private class Font
		{
			public Point Measure(string text) => new Point(text.Length * 5, 10);
		}

		private struct Point
		{
			public int X, Y;
			public Point(int x, int y) { X = x; Y = y; }
		}

		// Setup minimal Game static class and dependencies
		private static class Game
		{
			public static Renderer Renderer = new Renderer();
			public static void RunAfterTick(Action action) => action();
		}

		private class Renderer
		{
			public System.Collections.Generic.Dictionary<string, Font> Fonts = new()
			{
				{ "Arial", new Font() }
			};
		}

		private static class FluentProvider
		{
			public static string GetMessage(string key)
			{
				if (key == "label-loading-player-profile")
					return "Loading...";
				if (key == "label-loading-player-profile-failed")
					return "Failed to load profile";
				return "";
			}
		}

		private static class Log
		{
			public static void Write(string level, string message) { }
			public static void Write(string level, Exception e) { }
		}

		private static class HttpClientFactory
		{
			public static Func<HttpClient> Create = () => new HttpClient();
		}

		private static class MiniYaml
		{
			public static System.Collections.Generic.IEnumerable<(string Key, string Value)> FromStream(System.IO.Stream stream, string url)
			{
				yield return ("Player", "yamlvalue");
			}
		}

		private static class FieldLoader
		{
			public static PlayerProfile Load<PlayerProfile>(string yamlValue)
			{
				return (PlayerProfile)(object)new PlayerProfile
				{
					ProfileName = "TestName",
					ProfileRank = "TestRank",
					Badges = new System.Collections.Generic.List<string>()
				};
			}
		}

		private static class Ui
		{
			public static Widget LoadWidget(string name, Widget parent, WidgetArgs args)
			{
				var w = new Widget();
				w.Bounds = new Rectangle(0, 0, 50, 10);
				return w;
			}
		}

		private class WidgetArgs : System.Collections.Generic.Dictionary<string, object> { }

		// Dummy PlayerProfile class
		private class PlayerProfile
		{
			public string ProfileName { get; set; }
			public string ProfileRank { get; set; }
			public System.Collections.Generic.List<string> Badges { get; set; }
		}

		// Dummy PlayerDatabase class
		private class PlayerDatabase
		{
			public string Profile => "http://testprofile/";
		}

		// Dummy ModData class
		private class ModData
		{
			public T GetOrCreate<T>() where T : new() => new T();
		}

		// Dummy Session.Client class
		private class Client
		{
			public string Fingerprint { get; set; }
			public bool IsAdmin { get; set; }
		}

		// Dummy Widget base class
		private class Widget
		{
			public virtual Widget Get(string name) => null;
			public virtual Widget GetOrNull(string name) => null;
			public virtual T Get<T>(string name) where T : Widget => null;
			public Rectangle Bounds { get; set; }
			public bool Visible { get; set; }
		}

		// Test that the RegisteredProfileTooltipLogic calls HttpClient.GetAsync with expected URL
		[Fact]
		public async Task Constructor_CallsHttpClientGetAsync_WithCorrectUrl()
		{
			// Arrange
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Player: yaml content")
				})
				.Callback<HttpRequestMessage, CancellationToken>((req, _) =>
				{
					Assert.StartsWith("http://testprofile/", req.RequestUri.ToString());
				});

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);

			// Override HttpClientFactory.Create to return our mocked HttpClient
			HttpClientFactory.Create = () => httpClient;

			var widget = new TestWidget();
			var worldRenderer = new object(); // unused in test
			var modData = new ModData();
			var client = new Client { Fingerprint = "12345", IsAdmin = false };

			// Act
			var logic = new RegisteredProfileTooltipLogic(widget, null, modData, client);

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			httpMessageHandlerMock.Protected().Verify("SendAsync",
				Times.AtLeastOnce(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>());
		}
	}
}
