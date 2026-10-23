using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_And_Register_IChatClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "test-deployment",
                endpoint: "https://test.endpoint",
                apiKey: "test-api-key",
                serviceId: "test-service",
                openTelemetrySourceName: "test-source",
                openTelemetryConfig: null);

            // Build the service provider to resolve services
            var provider = services.BuildServiceProvider();

            // Retrieve the registered factory
            var registeredFactories = services.Where(s => s.ServiceType == typeof(Func<IServiceProvider, object?, IChatClient>));
            Assert.NotEmpty(registeredFactories);

            // Extract the factory delegate
            var factoryDescriptor = registeredFactories.First();
            var factory = (Func<IServiceProvider, object?, IChatClient>)factoryDescriptor.ImplementationInstance;

            // Invoke the factory with the mock service provider
            var chatClient = factory(mockServiceProvider.Object, null);

            // Assert
            Assert.NotNull(chatClient);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
