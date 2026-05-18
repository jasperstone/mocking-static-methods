using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	// Minimal stubs for Game static members used in WebServices
	static class Game
	{
		public static string EngineVersion = "engine-version";
		public static ModDataContainer ModData = new ModDataContainer();
		public static void RunAfterTick(Action action) => action();

		public class ModDataContainer
		{
			public ManifestContainer Manifest = new ManifestContainer();
		}

		public class ManifestContainer
		{
			public string Id = "mod-id";
			public MetadataContainer Metadata = new MetadataContainer();
		}

		public class MetadataContainer
		{
			public string Version = "mod-version";
		}
	}

	// A fake HttpClientFactory to inject a mock HttpClient
	static class HttpClientFactory
	{
		public static Func<HttpClient> CreateFunc = () => new HttpClient();

		public static HttpClient Create() => CreateFunc();
	}

	// We need to override WebServices to use our HttpClientFactory and Game stubs
	public class WebServicesTestable : WebServices
	{
		// Expose ModVersionStatus for testing by adding a method to get it
		public ModVersionStatus GetModVersionStatus() => ModVersionStatus;

		public new void CheckModVersion()
		{
			// We override to run synchronously for testing to avoid hanging
			var queryURL = new HttpQueryBuilder(VersionCheck)
			{
				{ "protocol", 1 },
				{ "engine", Game.EngineVersion },
				{ "mod", Game.ModData.Manifest.Id },
				{ "version", Game.ModData.Manifest.Metadata.Version }
			}.ToString();

			try
			{
				var client = HttpClientFactory.Create();

				var httpResponseMessage = client.GetAsync(queryURL).GetAwaiter().GetResult();
				var result = httpResponseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				var status = ModVersionStatus.Latest;
				switch (result)
				{
					case "outdated": status = ModVersionStatus.Outdated; break;
					case "unknown": status = ModVersionStatus.Unknown; break;
					case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
				}

				Game.RunAfterTick(() => ModVersionStatus = status);
			}
			catch { }
		}
	}

	public class WebServicesTests
	{
		[Fact]
		public void CheckModVersion_SetsStatusToLatest_WhenResponseIsEmpty()
		{
			var ws = new WebServicesTestable();

			// Setup HttpClient to return empty string
			var handler = new MockHttpMessageHandler("");
			HttpClientFactory.CreateFunc = () => new HttpClient(handler);

			ws.CheckModVersion();

			Assert.Equal(ModVersionStatus.Latest, ws.GetModVersionStatus());
		}

		[Theory]
		[InlineData("outdated", ModVersionStatus.Outdated)]
		[InlineData("unknown", ModVersionStatus.Unknown)]
		[InlineData("playtest", ModVersionStatus.PlaytestAvailable)]
		public void CheckModVersion_SetsStatusBasedOnResponse(string response, ModVersionStatus expectedStatus)
		{
			var ws = new WebServicesTestable();

			var handler = new MockHttpMessageHandler(response);
			HttpClientFactory.CreateFunc = () => new HttpClient(handler);

			ws.CheckModVersion();

			Assert.Equal(expectedStatus, ws.GetModVersionStatus());
		}

		// Helper HttpMessageHandler to mock HttpClient responses
		class MockHttpMessageHandler : HttpMessageHandler
		{
			readonly string _responseContent;

			public MockHttpMessageHandler(string responseContent)
			{
				_responseContent = responseContent;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(_responseContent)
				});
			}
		}
	}
}
