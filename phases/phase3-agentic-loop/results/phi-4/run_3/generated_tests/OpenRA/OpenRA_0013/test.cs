using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

public class ServerListLogicTests
{
    [Fact]
    public async Task RefreshServerList_CallsGetAsyncWithCorrectUrl()
    {
        // Arrange
        var servicesMock = new Mock<WebServices>();
        servicesMock.Setup(s => s.ServerList).Returns("http://example.com/serverlist");

        var gameMock = new Mock<Game>();
        gameMock.Setup(g => g.Spectators).Returns(0);
        gameMock.Setup(g => g.ProtocolVersion).Returns("1.0");
        gameMock.Setup(g => g.EngineVersion).Returns("1.0");
        gameMock.Setup(g => g.ModData).Returns(new ModData
        {
            Manifest = new Manifest
            {
                Id = "testmod",
                Metadata = new Metadata { Version = "1.0" }
            }
        });

        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("mock response")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var logic = new ServerListLogic(null, new ModData(), gameServer => { }, servicesMock.Object)
        {
            Game = gameMock.Object
        };

        // Act
        logic.RefreshServerList();

        // Assert
        await Task.Delay(100); // Give time for the async operation to complete

        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString() == "http://example.com/serverlist?protocol=1.0&engine=1.0&mod=testmod&version=1.0"
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
