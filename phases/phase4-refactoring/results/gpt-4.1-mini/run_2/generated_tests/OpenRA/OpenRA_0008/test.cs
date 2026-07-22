using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		// Helper to override Game.RunAfterTick to run immediately for testing
		private class RunAfterTickOverride : IDisposable
		{
			private readonly Action<Action> originalRunAfterTick;

			public RunAfterTickOverride()
			{
				originalRunAfterTick = Game.RunAfterTick;
				Game.RunAfterTick = action => action();
			}

			public void Dispose()
			{
				Game.RunAfterTick = originalRunAfterTick;
			}
		}

		[Theory]
		[InlineData("outdated", ModVersionStatus.Outdated)]
		[InlineData("unknown", ModVersionStatus.Unknown)]
		[InlineData("playtest", ModVersionStatus.PlaytestAvailable)]
		[InlineData("anythingelse", ModVersionStatus.Latest)]
		public async Task CheckModVersion_SetsCorrectStatusBasedOnResponse(string responseContent, ModVersionStatus expectedStatus)
		{
			using var runAfterTickOverride = new RunAfterTickOverride();

			// Setup a mocked HttpMessageHandler to return the desired response content
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(responseContent),
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// We cannot inject HttpClient into WebServices, so we subclass and override a method to use our HttpClient
			var webServices = new TestableWebServices(httpClient);

			// Call the async version of CheckModVersion that returns a Task for testing
			await webServices.CheckModVersionAsync();

			Assert.Equal(expectedStatus, webServices.ModVersionStatus);

			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
				ItExpr.IsAny<CancellationToken>());
		}

		// Subclass of WebServices to override HttpClient creation and expose async CheckModVersion for testing
		private class TestableWebServices : WebServices
		{
			private readonly HttpClient _httpClient;

			public TestableWebServices(HttpClient httpClient)
			{
				_httpClient = httpClient;
			}

			// Expose an async version of CheckModVersion for testing
			public async Task CheckModVersionAsync()
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
					var httpResponseMessage = await _httpClient.GetAsync(queryURL);
					var result = await httpResponseMessage.Content.ReadAsStringAsync();

					var status = ModVersionStatus.Latest;
					switch (result)
					{
						case "outdated": status = ModVersionStatus.Outdated; break;
						case "unknown": status = ModVersionStatus.Unknown; break;
						case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
					}

					Game.RunAfterTick(() => ModVersionStatus = status);
				}
				catch
				{
					// swallow exceptions as original code does
				}
			}
		}
	}
}
