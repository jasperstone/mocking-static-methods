using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Server
{
	public class MasterServerPingerTests
	{
		[Fact]
		public async Task UpdateMasterServer_PostAsyncCalledAndResponseProcessed()
		{
			// Arrange
			var advertiseUrl = "http://testserver/advertise";
			var postData = "test-post-data";

			// Setup HttpMessageHandler mock to intercept PostAsync call
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

			// Replace HttpClientFactory.Create to return HttpClient with mocked handler
			HttpClientFactory.SetHttpClientFactory(() => new HttpClient(handlerMock.Object));

			// Create minimal server stub with ModData.GetOrCreate<WebServices>().ServerAdvertise returning advertiseUrl
			var server = new ServerStub(advertiseUrl);

			var pinger = new MasterServerPinger();

			// Act
			// Call the private UpdateMasterServer method via reflection since it's private
			var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(method);

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
		}

		// Minimal stub classes to support the test
		private class ServerStub
		{
			public ModDataStub ModData { get; }

			public ServerStub(string advertiseUrl)
			{
				ModData = new ModDataStub(advertiseUrl);
			}
		}

		private class ModDataStub
		{
			private readonly string advertiseUrl;

			public ModDataStub(string advertiseUrl)
			{
				this.advertiseUrl = advertiseUrl;
			}

			public T GetOrCreate<T>() where T : class, new()
			{
				if (typeof(T) == typeof(WebServices))
					return new WebServices { ServerAdvertise = advertiseUrl } as T;
				throw new NotImplementedException();
			}
		}

		private class WebServices
		{
			public string ServerAdvertise { get; set; }
		}
	}
}
