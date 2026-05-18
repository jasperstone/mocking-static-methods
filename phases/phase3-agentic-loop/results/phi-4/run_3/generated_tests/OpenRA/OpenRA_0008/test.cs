using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

public class WebServicesTests
{
    [Fact]
    public async Task CheckModVersion_CallsGetAsyncWithCorrectUrl()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("latest")
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

        var httpClientFactoryMock = new Mock<Func<HttpClient>>();
        httpClientFactoryMock.Setup(_ => _.Invoke()).Returns(httpClient);

        var webServices = new WebServices();

        // Mock HttpClientFactory.Create
        var httpClientFactory = new HttpClientFactoryMock(httpClientFactoryMock.Object);
        webServices.HttpClientFactory = httpClientFactory.Create;

        var expectedUrl = "https://master.openra.net/versioncheck?protocol=1&engine=1.0.0&mod=TestMod&version=1.0.0";

        var tcs = new TaskCompletionSource<bool>();

        // Mock Game.RunAfterTick to complete the task
        var gameMock = new Mock<Game>();
        gameMock.Setup(g => g.RunAfterTick(It.IsAny<Action>()))
                .Callback<Action>(action => action())
                .Verifiable();

        webServices.Game = gameMock.Object;

        // Act
        webServices.CheckModVersion();
        tcs.SetResult(true);

        await tcs.Task;

        // Assert
        await handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri.ToString() == expectedUrl),
            ItExpr.IsAny<CancellationToken>()
        );

        gameMock.Verify();
    }
}

public class HttpClientFactoryMock
{
    private readonly Func<HttpClient> _httpClientFactory;

    public HttpClientFactoryMock(Func<HttpClient> httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Func<HttpClient> Create => _httpClientFactory;
}

public class Game
{
    public void RunAfterTick(Action action) => action();
}
