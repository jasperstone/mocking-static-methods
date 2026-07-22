using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OpenRA.Mods.Common;
using OpenRA.Support;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        private sealed class User
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("gamer")]
            public bool Gamer { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("press_user")]
            public bool PressUser { get; set; }

            [JsonPropertyName("developer")]
            public bool Developer { get; set; }

            [JsonPropertyName("username")]
            public string Username { get; set; }

            [JsonPropertyName("display_name")]
            public string DisplayName { get; set; }
        }

        private sealed class Root
        {
            [JsonPropertyName("user")]
            public User User { get; set; }
        }

        private class FakeHttpClient : HttpClient
        {
            public Func<HttpRequestMessage, Task<HttpResponseMessage>> SendAsyncFunc { get; set; } = _ => throw new NotImplementedException();

            public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken = default)
            {
                return SendAsyncFunc(request);
            }
        }

        [Fact]
        public async Task GetPlayerName_NoApiKey_DoesNotInvokeCallback()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            
            var callbackInvoked = false;
            var integration = new ItchIntegration();
            integration.GetPlayerName(_ => callbackInvoked = true);

            // Act
            await Task.Delay(200);

            // Assert
            Assert.False(callbackInvoked);
        }

        [Fact]
        public async Task GetPlayerName_ValidApiKeyValidResponseWithDisplayName_InvokesCallbackWithDisplayName()
        {
            // Arrange
            var apiKey = "test-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey);
            var expectedDisplayName = "Test Display Name";

            var jsonResponse = $$"""
            {
                "user": {
                    "display_name": "{{expectedDisplayName}}",
                    "username": "testuser"
                }
            }
            """;

            var httpClient = new FakeHttpClient();
            httpClient.SendAsyncFunc = request =>
            {
                Assert.Equal("Bearer test-key", request.Headers.Authorization?.ToString());
                Assert.Equal("https://itch.io/api/1/jwt/me", request.RequestUri.ToString());
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });
            };

            // Replace HttpClientFactory.Create - this will be called inside the method
            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => httpClient;

            try
            {
                var callbackResult = string.Empty;
                var integration = new ItchIntegration();
                integration.GetPlayerName(name => callbackResult = name);

                // Act
                await Task.Delay(500);

                // Assert
                Assert.Equal(expectedDisplayName, callbackResult);
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
            }
        }

        [Fact]
        public async Task GetPlayerName_ValidApiKeyValidResponseNoDisplayName_InvokesCallbackWithUsername()
        {
            // Arrange
            var apiKey = "test-key";
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey);
            var expectedUsername = "testuser";

            var jsonResponse = $$"""
            {
                "user": {
                    "username": "{{expectedUsername}}",
                    "display_name": ""
                }
            }
            """;

            var httpClient = new FakeHttpClient();
            httpClient.SendAsyncFunc = _ =>
                Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => httpClient;

            try
            {
                var callbackResult = string.Empty;
                var integration = new ItchIntegration();
                integration.GetPlayerName(name => callbackResult = name);

                // Act
                await Task.Delay(500);

                // Assert
                Assert.Equal(expectedUsername, callbackResult);
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
            }
        }

        [Fact]
        public async Task GetPlayerName_HttpError_DoesNotInvokeCallback()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");

            var httpClient = new FakeHttpClient();
            httpClient.SendAsyncFunc = _ => 
                Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest));

            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => httpClient;

            try
            {
                var callbackInvoked = false;
                var integration = new ItchIntegration();
                integration.GetPlayerName(_ => callbackInvoked = true);

                // Act
                await Task.Delay(500);

                // Assert
                Assert.False(callbackInvoked);
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
            }
        }

        [Fact]
        public async Task GetPlayerName_DeserializationFailure_DoesNotInvokeCallback()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");

            var httpClient = new FakeHttpClient();
            httpClient.SendAsyncFunc = _ => 
                Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
                });

            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => httpClient;

            try
            {
                var callbackInvoked = false;
                var integration = new ItchIntegration();
                integration.GetPlayerName(_ => callbackInvoked = true);

                // Act
                await Task.Delay(500);

                // Assert
                Assert.False(callbackInvoked);
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
            }
        }
    }
}
