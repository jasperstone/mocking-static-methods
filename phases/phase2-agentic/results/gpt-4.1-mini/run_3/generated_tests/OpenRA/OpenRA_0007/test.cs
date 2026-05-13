using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Server
{
	public class MasterServerPingerTests
	{
		// Helper to create a server mock with necessary properties
		private static Server.Server CreateServerMock(string advertiseUrl, bool advertiseOnline = true, bool advertiseOnLocalNetwork = false)
		{
			var modData = new ModDataMock(advertiseUrl);
			var settings = new ServerSettingsMock(advertiseOnline, advertiseOnLocalNetwork);
			return new Server.Server(modData, settings);
		}

		[Fact]
		public async Task UpdateMasterServer_PostAsyncCalledAndProcessesResponse()
		{
			// Arrange
			var advertiseUrl = "http://testserver/advertise";
			var postData = "post data";

			// Setup HttpClient mock to respond with a specific content
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Post &&
						req.RequestUri == new Uri(advertiseUrl)),
					ItExpr.IsAny<System.Threading.CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("[1]Error message from server")
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Override HttpClientFactory.Create to return our mocked HttpClient
			HttpClientFactory.SetHttpClientFactory(() => httpClient);

			var server = CreateServerMock(advertiseUrl);

			var pinger = new MasterServerPinger();

			// Use reflection to invoke private UpdateMasterServer method
			var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			// Act
			method.Invoke(pinger, new object[] { server, postData });

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(200);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post &&
					req.RequestUri == new Uri(advertiseUrl)),
				ItExpr.IsAny<System.Threading.CancellationToken>());

			// Cleanup override
			HttpClientFactory.SetHttpClientFactory(null);
		}

		// Minimal mocks for Server and related classes to support the test
		private class ModDataMock : OpenRA.Server.ModData
		{
			private readonly string advertiseUrl;
			public ModDataMock(string advertiseUrl)
			{
				this.advertiseUrl = advertiseUrl;
			}
			public override T GetOrCreate<T>()
			{
				if (typeof(T) == typeof(WebServices))
					return (T)(object)new WebServices { ServerAdvertise = advertiseUrl };
				return base.GetOrCreate<T>();
			}
		}

		private class ServerSettingsMock : OpenRA.Server.ServerSettings
		{
			public ServerSettingsMock(bool advertiseOnline, bool advertiseOnLocalNetwork)
			{
				AdvertiseOnline = advertiseOnline;
				AdvertiseOnLocalNetwork = advertiseOnLocalNetwork;
			}
		}
	}
}
