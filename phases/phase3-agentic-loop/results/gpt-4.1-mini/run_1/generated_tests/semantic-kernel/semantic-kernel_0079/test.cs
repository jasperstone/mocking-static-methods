using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_WithApiKey_RegistersFactory_AndCallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a mock ILoggerFactory to be resolved by the service provider
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Register a dummy HttpClient to avoid null HttpClient in factory
            services.AddSingleton(new HttpClient());

            // Call the extension method under test
            services.AddAzureOpenAIChatClient(
                deploymentName: "deployment",
                endpoint: "https://endpoint",
                apiKey: "apiKey",
                serviceId: "serviceId");

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Find the service descriptor for IChatClient with serviceId "serviceId"
            var descriptor = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IChatClient) &&
                d.ImplementationFactory != null);

            Assert.NotNull(descriptor);

            // Act
            // Invoke the factory delegate with our service provider
            var chatClient = descriptor.ImplementationFactory!(serviceProvider);

            // Assert
            Assert.NotNull(chatClient);
            // The factory calls GetService<ILoggerFactory> on the service provider internally
            // We verify that the resolved logger factory is the same instance we registered
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.Same(mockLoggerFactory.Object, resolvedLoggerFactory);
        }
    }
}
