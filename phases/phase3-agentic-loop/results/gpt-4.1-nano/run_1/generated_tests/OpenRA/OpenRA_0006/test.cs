using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenRA.Mods.Common;

public class ItchIntegrationTests
{
    [Fact]
    public void GetPlayerName_ShouldInvokeCallbackWithUsername_WhenApiReturnsValidUser()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var responseContent = new
        {
            user = new
            {
                url = "https://itch.io/user/testuser",
                gamer = true,
                id = 123,
                press_user = false,
                developer = false,
                username = "testuser",
                display_name = "Test User"
            }
        };
        var jsonResponse = JsonSerializer.Serialize(responseContent);
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        mockHttpMessageHandler
            .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.Create()).Returns(httpClient);

        // Set environment variable
        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

        var integration = new ItchIntegrationWithFactory(factoryMock.Object);

        string callbackResult = null;
        var callbackInvoked = new ManualResetEvent(false);
        void Callback(string name)
        {
            callbackResult = name;
            callbackInvoked.Set();
        }

        // Act
        integration.GetPlayerName(Callback);

        // Wait for async callback
        Assert.True(callbackInvoked.WaitOne(TimeSpan.FromSeconds(2)), "Callback was not invoked in time");

        // Assert
        Assert.Equal("Test User", callbackResult);
    }
}

// To facilitate dependency injection of HttpClientFactory
public class ItchIntegrationWithFactory : ItchIntegration
{
    private readonly IHttpClientFactory _factory;

    public ItchIntegrationWithFactory(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public new void GetPlayerName(Action<string> callback)
    {
        Task.Run(async () =>
        {
            User user = null;

            var apiKey = Environment.GetEnvironmentVariable("ITCHIO_API_KEY", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    var client = _factory.Create();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    var httpResponseMessage = await client.GetAsync("https://itch.io/api/1/jwt/me");
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var result = await httpResponseMessage.Content.ReadAsStringAsync();
                    user = JsonSerializer.Deserialize<Root>(result)?.User;
                }
                catch (Exception e)
                {
                    Log.Write("debug", "Failed to query player name from itch.io API.");
                    Log.Write("debug", e);
                }
            }

            if (user != null)
            {
                string name;
                if (string.IsNullOrEmpty(user.DisplayName))
                    name = user.Username;
                else
                    name = user.DisplayName;

                Game.RunAfterTick(() => callback?.Invoke(name));
            }
        });
    }
}
