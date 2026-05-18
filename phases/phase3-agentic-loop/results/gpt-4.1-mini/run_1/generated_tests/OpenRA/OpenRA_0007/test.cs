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
		// Helper to create a Server mock with minimal required properties
		private static Server.Server CreateServerMock(string advertiseUrl)
		{
			var modData = new ModDataMock(advertiseUrl);
			var server = new Server.Server
			{
				ModData = modData,
				Settings = new ServerSettingsMock
				{
					AdvertiseOnline = true,
					AdvertiseOnLocalNetwork = false
				},
				IsMultiplayer = true
			};
			return server;
		}

		[Fact]
		public async Task UpdateMasterServer_PostAsync_IsCalledAndProcessesResponse()
		{
			// Arrange
			var advertiseUrl = "http://testserver/advertise";
			var server = CreateServerMock(advertiseUrl);

			// Setup HttpClient mock to intercept PostAsync call
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Post && req.RequestUri.ToString() == advertiseUrl),
					ItExpr.IsAny<System.Threading.CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("[1]Test error message")
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			// Replace HttpClientFactory.Create to return our mocked HttpClient
			HttpClientFactory.SetFactory(() => httpClient);

			var pinger = new MasterServerPinger();

			// Act
			// We need to call the private UpdateMasterServer method via reflection because it's private
			var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Assert.NotNull(method);

			method.Invoke(pinger, new object[] { server, "postData" });

			// Wait a bit for the async Task.Run to complete
			await Task.Delay(100);

			// Assert
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Post && req.RequestUri.ToString() == advertiseUrl),
				ItExpr.IsAny<System.Threading.CancellationToken>());
		}

		// Minimal mocks for required types
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
			public override bool AdvertiseOnline { get; set; }
			public override bool AdvertiseOnLocalNetwork { get; set; }
		}
	}
}
