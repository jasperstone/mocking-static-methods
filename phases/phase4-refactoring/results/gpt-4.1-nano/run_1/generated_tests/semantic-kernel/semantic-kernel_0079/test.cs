using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SemanticKernel.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_And_Returns_IServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Act
            var result = services.AddAzureOpenAIChatClient(
                deploymentName: "testDeployment",
                endpoint: "https://test.endpoint",
                apiKey: "testApiKey",
                serviceId: "testService",
                modelId: "testModel",
                apiVersion: null,
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Assert
            Assert.NotNull(result);
            var serviceProvider = result.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }
    }
}
