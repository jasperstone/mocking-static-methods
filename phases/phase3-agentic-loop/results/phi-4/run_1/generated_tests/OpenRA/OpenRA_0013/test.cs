using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

public class ServerListLogicTests
{
    [Fact]
    public async Task RefreshServerList_CallsGetAsyncWithCorrectUrl()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var queryURL = "http://example.com/query";
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("mock response")
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        var servicesMock = new Mock<WebServices>();
        servicesMock.Setup(s => s.ServerList).Returns(queryURL);

        var modDataMock = new Mock<ModData>();
        modDataMock.Setup(m => m.GetOrCreate<WebServices>()).Returns(servicesMock.Object);

        var logic = new ServerListLogic(null, modDataMock.Object, _ => { });

        // Act
        logic.RefreshServerList();

        // Assert
        await Task.Delay(100); // Give time for the Task.Run to execute
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString() == queryURL),
            ItExpr.IsAny<CancellationToken>());
    }
}
