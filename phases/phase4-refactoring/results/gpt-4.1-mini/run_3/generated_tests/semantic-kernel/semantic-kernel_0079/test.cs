using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_RegistersIChatClientFactory_AndResolvesIChatClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory to the service collection to be resolved by the factory
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "testDeployment",
                endpoint: "https://test.endpoint",
                apiKey: "testApiKey");

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IChatClient from the service provider
            var chatClient = serviceProvider.GetService<IChatClient>();

            // Assert
            Assert.NotNull(chatClient);
        }
    }
}
