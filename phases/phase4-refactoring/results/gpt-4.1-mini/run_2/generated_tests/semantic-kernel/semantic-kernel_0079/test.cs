using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_RegistersFactory_AndCallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returnedServices = AzureOpenAIServiceCollectionExtensions.AddAzureOpenAIChatClient(
                services,
                deploymentName: "deployment",
                endpoint: "https://endpoint",
                apiKey: "key");

            // Assert
            Assert.Same(services, returnedServices);

            // Find the service descriptor for IChatClient keyed singleton registration
            var descriptor = Assert.Single(services, d => d.ServiceType.Name == "IChatClient");
            Assert.NotNull(descriptor);

            // Arrange a mock IServiceProvider to verify GetService call
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // The factory is stored as an implementation factory delegate
            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Act - invoke the factory to trigger GetService call
            var client = factory!(mockServiceProvider.Object);

            // Assert that GetService was called on IServiceProvider
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            Assert.NotNull(client);
        }
    }
}
