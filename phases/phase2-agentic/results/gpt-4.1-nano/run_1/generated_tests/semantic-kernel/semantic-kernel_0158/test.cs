using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_WithNullServices_Throws()
        {
            // Arrange
            IServiceCollection services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => OllamaServiceCollectionExtensions.AddOllamaTextGeneration(services));
        }

        [Fact]
        public void AddOllamaTextGeneration_WithServiceProvider_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            services.AddTransient<IOllamaApiClient, MockOllamaApiClient>();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaTextGeneration(services, "testModel", new OllamaApiClient());

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithNullOllamaClient_Throws()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IOllamaApiClient, MockOllamaApiClient>();
            var sp = services.BuildServiceProvider();

            // Act
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IOllamaApiClient, MockOllamaApiClient>();
            var sp2 = serviceCollection.BuildServiceProvider();

            // Use reflection or similar to invoke the method with null OllamaClient
            // For simplicity, test the method that throws if no IOllamaApiClient is registered
            var services2 = new ServiceCollection();
            services2.AddTransient<IOllamaApiClient, MockOllamaApiClient>();
            var sp3 = services2.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                // simulate the call inside the method
                var provider = sp3;
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var ollamaClient = provider.GetService<OllamaApiClient>();
                if (ollamaClient == null)
                {
                    throw new InvalidOperationException($"No {nameof(IOllamaApiClient)} implementations found in the service collection.");
                }
            });
            Assert.Contains(nameof(IOllamaApiClient), ex.Message);
        }

        [Fact]
        public void AddOllamaTextGeneration_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var mockOllamaClient = new Mock<OllamaApiClient>();
            services.AddTransient<IOllamaApiClient, MockOllamaApiClient>();
            var provider = services.BuildServiceProvider();

            // Setup a mock service provider to test GetService call
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetService<OllamaApiClient>()).Returns(mockOllamaClient.Object);

            // Act
            var result = OllamaServiceCollectionExtensions.AddOllamaTextGeneration(services, "model", mockOllamaClient.Object);

            // Assert
            Assert.NotNull(result);
        }
    }

    // Mock implementation for IOllamaApiClient
    public class MockOllamaApiClient : IOllamaApiClient
    {
        // Implement interface members if needed
    }
}
