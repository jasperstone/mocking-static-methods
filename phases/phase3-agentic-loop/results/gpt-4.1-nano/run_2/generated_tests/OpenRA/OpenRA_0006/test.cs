using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenRA.Mods.Common;

public class ItchIntegrationTests
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handlerFunc(request));
        }
    }

    [Fact]
    public void GetPlayerName_ShouldInvokeCallbackWithDisplayName_WhenUserHasDisplayName()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(req =>
        {
            var apiResponse = new
            {
                user = new
                {
                    url = "https://itch.io/user/testuser",
                    gamer = true,
                    id = 123,
                    press_user = false,
                    developer = true,
                    username = "testuser",
                    display_name = "Test User"
                }
            };
            var jsonResponse = JsonSerializer.Serialize(apiResponse);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            return response;
        });
        var httpClient = new HttpClient(handler);
        var integration = new ItchIntegration(httpClient);

        // Set environment variable
        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

        string callbackResult = null;
        var callbackInvoked = new ManualResetEvent(false);
        Action<string> callback = (name) =>
        {
            callbackResult = name;
            callbackInvoked.Set();
        };

        // Act
        integration.GetPlayerName(callback);
        callbackInvoked.WaitOne(2000);

        // Assert
        Assert.Equal("Test User", callbackResult);
    }

    [Fact]
    public void GetPlayerName_ShouldInvokeCallbackWithUsername_WhenDisplayNameIsNullOrEmpty()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(req =>
        {
            var apiResponse = new
            {
                user = new
                {
                    url = "https://itch.io/user/testuser",
                    gamer = true,
                    id = 123,
                    press_user = false,
                    developer = true,
                    username = "testuser",
                    display_name = ""
                }
            };
            var jsonResponse = JsonSerializer.Serialize(apiResponse);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse)
            };
            return response;
        });
        var httpClient = new HttpClient(handler);
        var integration = new ItchIntegration(httpClient);

        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key");

        string callbackResult = null;
        var callbackInvoked = new ManualResetEvent(false);
        Action<string> callback = (name) =>
        {
            callbackResult = name;
            callbackInvoked.Set();
        };

        // Act
        integration.GetPlayerName(callback);
        callbackInvoked.WaitOne(2000);

        // Assert
        Assert.Equal("testuser", callbackResult);
    }

    [Fact]
    public void GetPlayerName_ShouldNotInvokeCallback_WhenApiKeyIsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
        var handler = new TestHttpMessageHandler(req => throw new Exception("Should not be called"));
        var httpClient = new HttpClient(handler);
        var integration = new ItchIntegration(httpClient);

        string callbackResult = null;
        var callbackInvoked = new ManualResetEvent(false);
        Action<string> callback = (name) =>
        {
            callbackResult = name;
            callbackInvoked.Set();
        };

        // Act
        integration.GetPlayerName(callback);
        var signaled = callbackInvoked.WaitOne(500);

        // Assert
        Assert.False(signaled);
        Assert.Null(callbackResult);
    }
}
