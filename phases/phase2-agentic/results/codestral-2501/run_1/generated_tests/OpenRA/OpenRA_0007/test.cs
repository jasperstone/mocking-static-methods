using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

public class MasterServerPingerTests
{
    [Fact]
    public async Task UpdateMasterServer_PostAsync_Success()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("[0]Success")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var mockServer = new Mock<S>();
        var mockWebServices = new Mock<WebServices>();
        mockWebServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
        mockServer.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(mockWebServices.Object);

        var pinger = new MasterServerPinger();

        // Act
        await pinger.UpdateMasterServer(mockServer.Object, "postData");

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://example.com") &&
                req.Content.ReadAsStringAsync().Result == "postData"
            ),
            ItExpr.IsAny<System.Threading.CancellationToken>()
        );
    }

    [Fact]
    public async Task UpdateMasterServer_PostAsync_Failure()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException());

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var mockServer = new Mock<S>();
        var mockWebServices = new Mock<WebServices>();
        mockWebServices.Setup(ws => ws.ServerAdvertise).Returns(new Uri("http://example.com"));
        mockServer.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(mockWebServices.Object);

        var pinger = new MasterServerPinger();

        // Act
        await pinger.UpdateMasterServer(mockServer.Object, "postData");

        // Assert
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == new Uri("http://example.com") &&
                req.Content.ReadAsStringAsync().Result == "postData"
            ),
            ItExpr.IsAny<System.Threading.CancellationToken>()
        );
    }
}
