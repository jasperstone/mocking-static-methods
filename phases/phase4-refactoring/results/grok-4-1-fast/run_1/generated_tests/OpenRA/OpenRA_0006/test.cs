using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Support;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public async Task GetPlayerName_NoApiKey_DoesNotInvokeCallback()
        {
            // Arrange
            var callbackFired = false;
            Action<string> callback = name => callbackFired = true;
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null);
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(callback);
            await Task.Delay(500);

            // Assert
            Assert.False(callbackFired);
        }

        [Fact]
        public async Task GetPlayerName_ValidApiKeyWithDisplayName_InvokesCallbackWithDisplayName()
        {
            // Arrange
            var receivedName = (string)null;
            Action<string> callback = name => receivedName = name;
            
            // Mock HttpClientFactory by replacing its static instance via reflection
            var mockClient = CreateMockHttpClient("{\"user\":{\"display_name\":\"TestDisplay\",\"username\":\"testuser\"}}");
            
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(callback);
            await Task.Delay(500);

            // Assert
            Assert.Equal("TestDisplay", receivedName);
        }

        [Fact]
        public async Task GetPlayerName_ValidApiKeyNoDisplayName_InvokesCallbackWithUsername()
        {
            // Arrange
            var receivedName = (string)null;
            Action<string> callback = name => receivedName = name;
            
            var mockClient = CreateMockHttpClient("{\"user\":{\"username\":\"testuser\"}}");
            
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(callback);
            await Task.Delay(500);

            // Assert
            Assert.Equal("testuser", receivedName);
        }

        [Fact]
        public async Task GetPlayerName_HttpError_DoesNotInvokeCallback()
        {
            // Arrange
            var callbackFired = false;
            Action<string> callback = name => callbackFired = true;
            
            CreateMockHttpClient("", System.Net.HttpStatusCode.BadRequest);
            
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-key");
            var integration = new ItchIntegration();

            // Act
            integration.GetPlayerName(callback);
            await Task.Delay(500);

            // Assert
            Assert.False(callbackFired);
        }

        private HttpClient CreateMockHttpClient(string jsonResponse, System.Net.HttpStatusCode statusCode = System.Net.HttpStatusCode.OK)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var mockClient = new HttpClient(handler.Object);
            
            // Replace HttpClientFactory's internal static client via reflection
            var factoryType = Type.GetType("OpenRA.Core.HttpClientFactory, OpenRA.Core") 
                           ?? Type.GetType("OpenRA.Platform.HttpClientFactory, OpenRA.Platform");
            
            if (factoryType != null)
            {
                var field = factoryType.GetField("client", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                field?.SetValue(null, mockClient);
            }
            
            return mockClient;
        }
    }
}
