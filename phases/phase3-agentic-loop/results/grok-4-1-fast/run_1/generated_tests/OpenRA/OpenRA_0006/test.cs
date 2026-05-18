using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        private Mock<HttpMessageHandler> mockHandler;
        private HttpClient mockHttpClient;
        private bool callbackCalled;
        private string callbackResult;

        private void ResetCallback()
        {
            callbackCalled = false;
            callbackResult = "";
        }

        private Action<string> GetCallback()
        {
            return name =>
            {
                callbackCalled = true;
                callbackResult = name;
            };
        }

        private void SetupHttpClient(string responseJson)
        {
            mockHandler = new Mock<HttpMessageHandler>();
            mockHttpClient = new HttpClient(mockHandler.Object);
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson)
                });
        }

        private void SetupHttpClientFailure()
        {
            mockHandler = new Mock<HttpMessageHandler>();
            mockHttpClient = new HttpClient(mockHandler.Object);
            
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API failure"));
        }

        [Fact]
        public async Task GetPlayerName_CallsCallbackWithDisplayName_WhenApiSuccessful()
        {
            // Arrange
            ResetCallback();
            var fakeResponse = JsonSerializer.Serialize(new { user = new { display_name = "TestDisplay", username = "testuser" } });
            SetupHttpClient(fakeResponse);

            // Use reflection to replace HttpClientFactory.Create static method
            var httpClientFactoryType = Type.GetType("OpenRA.Support.HttpClientFactory, OpenRA") ?? 
                                       Type.GetType("OpenRA.Core.HttpClientFactory, OpenRA") ??
                                       Type.GetType("OpenRA.HttpClientFactory, OpenRA");
            
            if (httpClientFactoryType != null)
            {
                var createMethod = httpClientFactoryType.GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                var originalMethod = createMethod?.CreateDelegate(typeof(Func<HttpClient>));
                
                // Use a wrapper to capture the call
                HttpClient capturedClient = null;
                var factoryType = typeof(ItchIntegrationTests);
                var wrapperMethod = factoryType.GetMethod("CreateWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                
                createMethod?.CreateDelegate(typeof(Func<HttpClient>), wrapperMethod?.CreateDelegate(null, mockHttpClient));
            }

            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "fakekey");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(GetCallback());
            await Task.Delay(500);

            // Cleanup
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);

            // Assert
            Assert.True(callbackCalled);
            Assert.Equal("TestDisplay", callbackResult);
        }

        [Fact]
        public async Task GetPlayerName_CallsCallbackWithUsername_WhenNoDisplayName()
        {
            // Arrange
            ResetCallback();
            var fakeResponse = JsonSerializer.Serialize(new { user = new { username = "testuser" } });
            SetupHttpClient(fakeResponse);

            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "fakekey");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(GetCallback());
            await Task.Delay(500);

            // Cleanup
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);

            // Assert
            Assert.True(callbackCalled);
            Assert.Equal("testuser", callbackResult);
        }

        [Fact]
        public async Task GetPlayerName_NoCallbackCall_WhenApiFails()
        {
            // Arrange
            ResetCallback();
            SetupHttpClientFailure();

            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "fakekey");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(GetCallback());
            await Task.Delay(500);

            // Cleanup
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);

            // Assert
            Assert.False(callbackCalled);
        }

        [Fact]
        public async Task GetPlayerName_NoCallbackCall_WhenNoApiKey()
        {
            // Arrange
            ResetCallback();
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(GetCallback());
            await Task.Delay(300);

            // Assert
            Assert.False(callbackCalled);
        }

        // Wrapper method for HttpClientFactory replacement
        private static HttpClient CreateWrapper()
        {
            return mockHttpClient;
        }
    }
}
