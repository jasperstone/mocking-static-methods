using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Tests
{
	public class MasterServerPingerTests
	{
		[Fact]
		public async Task UpdateMasterServer_SendsPostRequestToMasterServer()
		{
			// Arrange
			var server = new Mock<S>();
			var webServices = new WebServices { ServerAdvertise = "https://example.com" };
			server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices);
			var gameServer = new GameServer(server.Object);
			var postData = gameServer.ToPOSTData(false);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			var httpClient = new Mock<HttpClient>();
			httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

			var masterServerPinger = new MasterServerPinger();

			// Act
			await masterServerPinger.UpdateMasterServer(server.Object, postData);

			// Assert
			httpClient.Verify(c => c.PostAsync(webServices.ServerAdvertise, It.IsAny<StringContent>()), Times.Once);
		}

		[Fact]
		public async Task UpdateMasterServer_HandlesSuccessfulResponse()
		{
			// Arrange
			var server = new Mock<S>();
			var webServices = new WebServices { ServerAdvertise = "https://example.com" };
			server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices);
			var gameServer = new GameServer(server.Object);
			var postData = gameServer.ToPOSTData(false);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			var httpClient = new Mock<HttpClient>();
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[0]Connected") };
			httpClient.Setup(c => c.PostAsync(webServices.ServerAdvertise, It.IsAny<StringContent>())).ReturnsAsync(response);
			httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

			var masterServerPinger = new MasterServerPinger();

			// Act
			await masterServerPinger.UpdateMasterServer(server.Object, postData);

			// Assert
			Assert.False(masterServerPinger.isInitialPing);
		}

		[Fact]
		public async Task UpdateMasterServer_HandlesErrorResponse()
		{
			// Arrange
			var server = new Mock<S>();
			var webServices = new WebServices { ServerAdvertise = "https://example.com" };
			server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices);
			var gameServer = new GameServer(server.Object);
			var postData = gameServer.ToPOSTData(false);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			var httpClient = new Mock<HttpClient>();
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("[1]Error message") };
			httpClient.Setup(c => c.PostAsync(webServices.ServerAdvertise, It.IsAny<StringContent>())).ReturnsAsync(response);
			httpClientFactory.Setup(f => f.CreateClient()).Returns(httpClient.Object);

			var masterServerPinger = new MasterServerPinger();

			// Act
			await masterServerPinger.UpdateMasterServer(server.Object, postData);

			// Assert
			Assert.False(masterServerPinger.isInitialPing);
		}
	}
}
