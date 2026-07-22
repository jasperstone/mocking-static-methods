using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_ResolvesIChatClient_AndCallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a NullLoggerFactory to satisfy ILoggerFactory dependency
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "testDeployment",
                endpoint: "https://testendpoint",
                apiKey: "testApiKey");

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IChatClient service (default serviceId is null)
            var chatClient = serviceProvider.GetService<IChatClient>();

            // Assert
            Assert.NotNull(chatClient);
        }
    }
}
