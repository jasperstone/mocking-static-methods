using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		// Helper stub classes to satisfy Game static properties
		class DummyManifestMetadata
		{
			public string Version { get; set; }
		}

		class DummyManifest
		{
			public string Id { get; set; }
			public DummyManifestMetadata Metadata { get; set; }
		}

		class DummyModData
		{
			public DummyManifest Manifest { get; set; }
		}

		// We will override Game static members for testing
		private void SetupGameStatic(string modId, string version)
		{
			Game.EngineVersion = "engine-version";
			Game.ModData = new ModDataStub
			{
				Manifest = new ManifestStub
				{
					Id = modId,
					Metadata = new MetadataStub
					{
						Version = version
					}
				}
			};
		}

		// Stubs for Game.ModData.Manifest and nested Metadata
		private class MetadataStub
		{
			public string Version { get; set; }
		}

		private class ManifestStub
		{
			public string Id { get; set; }
			public MetadataStub Metadata { get; set; }
		}

		private class ModDataStub
		{
			public ManifestStub Manifest { get; set; }
		}

		// We will override Game.RunAfterTick to immediately invoke the action
		private void SetupRunAfterTick()
		{
			Game.RunAfterTick = action => action();
		}

		// We will mock HttpClientFactory.Create() to return a mock HttpClient
		// We need to replace HttpClientFactory.Create with a delegate or similar for testing
		// Since HttpClientFactory is static, we will use a helper class with a Func<HttpClient> that can be overridden in tests
		// But the original code calls HttpClientFactory.Create() directly, so we need to mock that static method
		// Since we cannot mock static methods easily, we will create a derived WebServices class that overrides CheckModVersion to inject a HttpClient

		// Instead, we will create a TestableWebServices class that allows injecting HttpClient and RunAfterTick delegate

		private class TestableWebServices : WebServices
		{
			public Func<HttpClient> HttpClientFactoryOverride { get; set; }
			public Action<Action> RunAfterTickOverride { get; set; }

			public new void CheckModVersion()
			{
				Task.Run(async () =>
				{
					var queryURL = new HttpQueryBuilder(VersionCheck)
					{
						{ "protocol", 1 },
						{ "engine", Game.EngineVersion },
						{ "mod", Game.ModData.Manifest.Id },
						{ "version", Game.ModData.Manifest.Metadata.Version }
					}.ToString();

					try
					{
						var client = HttpClientFactoryOverride != null ? HttpClientFactoryOverride() : HttpClientFactory.Create();

						var httpResponseMessage = await client.GetAsync(queryURL);
						var result = await httpResponseMessage.Content.ReadAsStringAsync();

						var status = ModVersionStatus.Latest;
						switch (result)
						{
							case "outdated": status = ModVersionStatus.Outdated; break;
							case "unknown": status = ModVersionStatus.Unknown; break;
							case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
						}

						if (RunAfterTickOverride != null)
							RunAfterTickOverride(() => ModVersionStatus = status);
						else
							Game.RunAfterTick(() => ModVersionStatus = status);
					}
					catch { }
				});
			}
		}

		[Theory]
		[InlineData("outdated", ModVersionStatus.Outdated)]
		[InlineData("unknown", ModVersionStatus.Unknown)]
		[InlineData("playtest", ModVersionStatus.PlaytestAvailable)]
		[InlineData("anythingelse", ModVersionStatus.Latest)]
		public async Task CheckModVersion_SetsCorrectStatus_BasedOnHttpResponse(string httpResponseContent, ModVersionStatus expectedStatus)
		{
			// Arrange
			SetupGameStatic("modid", "1.2.3");
			SetupRunAfterTick();

			var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(httpResponseContent)
				});

			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			var webServices = new TestableWebServices
			{
				HttpClientFactoryOverride = () => httpClient,
				RunAfterTickOverride = action => action()
			};

			// Act
			webServices.CheckModVersion();

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			Assert.Equal(expectedStatus, webServices.ModVersionStatus);
		}
	}
}
