using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using OpenRA.Mods.Common;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        private sealed class User
        {
            public string Url { get; set; }
            public bool Gamer { get; set; }
            public int Id { get; set; }
            public bool PressUser { get; set; }
            public bool Developer { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
        }

        private sealed class Root
        {
            public User User { get; set; }
        }

        private static string BuildUserJson(string displayName, string username)
        {
            return $$"""
            {
                "user": {
                    "url": "https://testuser.itch.io",
                    "gamer": false,
                    "id": 123,
                    "press_user": false,
                    "developer": false,
                    "username": "{{username}}",
                    "display_name": "{{displayName ?? ""}}"
                }
            }
            """;
        }

        private HttpClient CreateMockHttpClient(string responseJson)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "https://itch.io/api/1/jwt/me"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson)
                })
                .Verifiable();

            return new HttpClient(handlerMock.Object);
        }

        [Fact]
        public async Task GetPlayerName_CallsCallbackWithDisplayName_WhenApiReturnsUserWithDisplayName()
        {
            // Arrange
            var expectedName = "TestDisplayName";
            var userJson = BuildUserJson(expectedName, "testuser");
            var mockClient = CreateMockHttpClient(userJson);
            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => mockClient;
            var tcs = new TaskCompletionSource<bool>();
            var actualName = (string)null;

            try
            {
                var integration = new ItchIntegration();
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");

                integration.GetPlayerName(name =>
                {
                    actualName = name;
                    tcs.SetResult(true);
                });

                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            }

            // Assert - covers GetAsync call on line 66 with success path
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public async Task GetPlayerName_CallsCallbackWithUsername_WhenApiReturnsUserWithoutDisplayName()
        {
            // Arrange
            var expectedName = "TestUsername";
            var userJson = BuildUserJson(null, expectedName);
            var mockClient = CreateMockHttpClient(userJson);
            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => mockClient;
            var tcs = new TaskCompletionSource<bool>();
            var actualName = (string)null;

            try
            {
                var integration = new ItchIntegration();
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");

                integration.GetPlayerName(name =>
                {
                    actualName = name;
                    tcs.SetResult(true);
                });

                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(2));
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            }

            // Assert - covers GetAsync call on line 66 with success path
            Assert.Equal(expectedName, actualName);
        }

        [Fact]
        public async Task GetPlayerName_DoesNotCallCallback_WhenNoApiKeySet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            var callbackCalled = false;
            var integration = new ItchIntegration();
            var tcs = new TaskCompletionSource<bool>();

            // Act
            integration.GetPlayerName(_ => 
            {
                callbackCalled = true;
                tcs.TrySetResult(true);
            });

            // Wait a bit for task to complete (or not)
            try
            {
                await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
            }
            catch
            {
                // Expected if callback not called
            }

            // Assert
            Assert.False(callbackCalled);
        }

        [Fact]
        public async Task GetPlayerName_HandlesHttpFailure_WithoutCrashing()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Test failure"))
                .Verifiable();

            var mockClient = new HttpClient(handlerMock.Object);
            var originalCreate = HttpClientFactory.Create;
            HttpClientFactory.Create = () => mockClient;
            var callbackCalled = false;
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                var integration = new ItchIntegration();
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");

                // Act
                integration.GetPlayerName(_ => 
                {
                    callbackCalled = true;
                    tcs.TrySetResult(true);
                });

                // Wait a bit for task to complete (or not)
                try
                {
                    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
                }
                catch
                {
                    // Expected if callback not called
                }
            }
            finally
            {
                HttpClientFactory.Create = originalCreate;
                Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            }

            // Assert - covers exception path around GetAsync line 66
            Assert.False(callbackCalled);
        }
    }
}
