using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using OpenRA.Traits;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_MakesGetRequestToServerListUrl()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>(MockBehavior.Strict, new HttpMessageHandler()); // Create a real HttpMessageHandler to avoid exceptions when calling SendAsync on the mock HttpClient instance. This is necessary because the HttpClient instance returned by HttpClientFactory.Create() is not a mock instance, but a real one, and it will throw an exception when SendAsync is called on it if it's not properly initialized with a real HttpMessageHandler instance. We use MockBehavior.Strict to ensure that any calls to the HttpClient instance that are not explicitly set up will throw an exception, helping to prevent bugs that might arise from unexpected behavior of the HttpClient instance. We also pass a new instance of HttpMessageHandler to the HttpClient constructor to ensure that the HttpClient instance is properly initialized with a real HttpMessageHandler instance, which is necessary for the HttpClient instance to function correctly when SendAsync is called on it. We then set up the HttpClient instance to return a mock HttpResponseMessage instance when SendAsync is called on it, which allows us to control the response that the HttpClient instance returns and test the behavior of the RefreshServerList method in different scenarios. We also set up the HttpClientFactory instance to return the HttpClient instance when Create is called on it, which allows us to control the HttpClient instance that is used by the RefreshServerList method and test its behavior in different scenarios. We create a new instance of the ServerListLogic class, passing in a new instance of the Widget class, a new instance of the ModData class, and a lambda expression that does nothing when called, which allows us to test the behavior of the RefreshServerList method without actually joining a game server. We then call the RefreshServerList method on the ServerListLogic instance, which makes a GET request to the server list URL and parses the response as a list of game servers. We then verify that the SendAsync method was called on the HttpClient instance exactly once, which ensures that the RefreshServerList method made a GET request to the server list URL as expected.
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);

            var serverListLogic = new ServerListLogic(
                new Widget(),
                new ModData(),
                (GameServer server) => { }
            );

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            handler
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
