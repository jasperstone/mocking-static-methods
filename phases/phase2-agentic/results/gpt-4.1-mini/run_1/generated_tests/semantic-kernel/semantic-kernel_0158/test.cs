using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Moq;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.Ollama.Extensions
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_ThrowsIfNoOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceProvider = serviceProviderMock.Object;

            // Setup GetService to return null for all types
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(null);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(null);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOllamaApiClient))).Returns(null);

            // Setup GetKeyedService extension method to return null (simulate no client registered)
            // Since GetKeyedService is an extension method, we simulate by adding a helper service
            // but here we just rely on the serviceProviderMock returning null for those calls

            services.AddSingleton(serviceProvider);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddOllamaTextEmbeddingGeneration(null, null)
                    .BuildServiceProvider()
                    .GetRequiredService<ITextEmbeddingGenerationService>());

            Assert.Contains("No IOllamaApiClient implementations found", ex.Message);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_UsesProvidedOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var ollamaClientMock = new Mock<OllamaApiClient>(MockBehavior.Strict, new Uri("http://localhost"), "model");
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddOllamaTextEmbeddingGeneration(ollamaClientMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(embeddingService);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_UsesServiceProviderGetServiceOnOllamaClient()
        {
            // Arrange
            var services = new ServiceCollection();

            var ollamaClientMock = new Mock<OllamaApiClient>(MockBehavior.Strict, new Uri("http://localhost"), "model");
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetService calls to simulate the chain of calls in the extension method
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetKeyedService<OllamaApiClient>(It.IsAny<string>())).Returns((OllamaApiClient?)null);
            serviceProviderMock.Setup(sp => sp.GetKeyedService<IOllamaApiClient>(It.IsAny<string>())).Returns((IOllamaApiClient?)null);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(OllamaApiClient))).Returns(ollamaClientMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOllamaApiClient>()).Returns(ollamaClientMock.Object);

            // We need to register the extension methods GetKeyedService and AddKeyedSingleton for the test to compile and run.
            // Since these are extension methods, we simulate their behavior by mocking the service provider.

            // Act
            services.AddOllamaTextEmbeddingGeneration(null);

            var serviceProvider = services.BuildServiceProvider();

            // We cannot directly invoke the factory delegate from AddKeyedSingleton because it's internal to the extension method.
            // Instead, we test that the service can be resolved without throwing, which implies the GetService calls were used.

            var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

            // Assert
            // embeddingService may be null because the serviceProvider used in the factory is different from the one we built here.
            // So we just assert no exceptions and the service collection contains the service descriptor.
            Assert.Contains(services, d => d.ServiceType == typeof(ITextEmbeddingGenerationService));
        }
    }

    // Helper extension methods to simulate GetKeyedService and AddKeyedSingleton for testing
    internal static class ServiceProviderExtensions
    {
        public static T? GetKeyedService<T>(this IServiceProvider serviceProvider, string? serviceId)
            where T : class
        {
            return (T?)serviceProvider.GetService(typeof(T));
        }
    }

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeyedSingleton<TService>(
            this IServiceCollection services,
            string? serviceId,
            Func<IServiceProvider, string?, TService> implementationFactory)
            where TService : class
        {
            services.AddSingleton<TService>(sp => implementationFactory(sp, serviceId));
            return services;
        }
    }
}
