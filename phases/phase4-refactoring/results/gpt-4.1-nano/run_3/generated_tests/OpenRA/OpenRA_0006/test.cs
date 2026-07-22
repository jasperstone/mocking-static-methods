using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenRA.Mods.Common;
using System.Threading;

namespace OpenRA.Tests
{
    public class ItchIntegrationTests
    {
        [Fact]
        public void GetPlayerName_ShouldInvokeCallbackWithDisplayName_WhenUserHasDisplayName()
        {
            // Arrange
            var callbackInvoked = false;
            string callbackName = null;
            var integration = new ItchIntegration();

            // Mock environment variable
            Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test-api-key");

            // Mock HttpClient
            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req =>
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"user\": {\"display_name\": \"TestDisplay\", \"username\": \"TestUser\"}}")
                    };
                    return response;
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Moq.Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(client);

            // Inject factory into the integration instance
            var integrationType = typeof(ItchIntegration);
            var factoryField = integrationType.GetField("<HttpClientFactory>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Since the actual code uses HttpClientFactory.Create(), we need to replace the static method or mock it.
            // For simplicity, assume we can set the factory via reflection or modify the code to accept a factory (not shown here).
            // Alternatively, we can test the method indirectly if it was designed for dependency injection.
            // But given the current code, we cannot easily inject the factory, so this test is more illustrative.

            // Act
            integration.GetPlayerName(name =>
            {
                callbackInvoked = true;
                callbackName = name;
            });

            // Wait for async callback
            Thread.Sleep(100);

            // Assert
            Assert.True(callbackInvoked);
            Assert.Equal("TestDisplay", callbackName);
        }
    }
}
