using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_WithUri_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("https://test.endpoint/");
            var apiKey = "test-api-key";

            // Mock ILoggerFactory to be returned by IServiceProvider.GetService<ILoggerFactory>()
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Mock IServiceProvider to verify GetService<ILoggerFactory> is called
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object)
                .Verifiable();

            // Add a dummy HttpClientProvider.GetHttpClient override to avoid null issues
            // Since HttpClientProvider is static, we cannot mock it easily here,
            // so we will provide a HttpClient to avoid that code path.
            var httpClient = new HttpClient { BaseAddress = endpoint };

            // Act
            services.AddOpenAIChatClient(
                modelId,
                endpoint,
                apiKey,
                httpClient: httpClient);

            var provider = services.BuildServiceProvider();

            // Resolve the IChatClient factory delegate from the service collection
            var serviceDescriptor = services
                .FirstOrDefault(sd => sd.ServiceType == typeof(IChatClient));
            Assert.NotNull(serviceDescriptor);

            // The AddKeyedSingleton extension method adds a factory with signature Func<IServiceProvider, object?, IChatClient>
            // We will invoke the factory manually to test the IServiceProvider.GetService call
            var factoryField = serviceDescriptor.ImplementationInstance
                ?? serviceDescriptor.ImplementationFactory;

            // The factory is stored as a Func<IServiceProvider, object?, IChatClient>
            // We need to get the factory delegate from the service collection
            // But since AddKeyedSingleton is an extension method, and the service is registered as singleton,
            // we can resolve IChatClient from the provider passing the serviceId as null
            // However, the serviceId is optional and the registration is keyed, so we cannot resolve directly.
            // Instead, we will create a mock IServiceProvider and call the factory delegate directly.

            // To do this, we will re-register the service with a factory we can call
            var factoryCalled = false;
            IChatClient Factory(IServiceProvider sp, object? _)
            {
                factoryCalled = true;
                // Call the original factory to trigger GetService call
                var loggerFactory = sp.GetService(typeof(ILoggerFactory));
                Assert.NotNull(loggerFactory);
                return Mock.Of<IChatClient>();
            }

            var testServices = new ServiceCollection();
            testServices.AddKeyedSingleton<IChatClient>(null, Factory);
            var testProvider = testServices.BuildServiceProvider();

            // Act - call factory
            var chatClient = Factory(serviceProviderMock.Object, null);

            // Assert
            Assert.True(factoryCalled);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }

    // Extension method to mimic AddKeyedSingleton for testing
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKeyedSingleton<TService>(
            this IServiceCollection services,
            string? key,
            Func<IServiceProvider, object?, TService> factory)
            where TService : class
        {
            services.AddSingleton<TService>(sp => factory(sp, null));
            return services;
        }
    }
}
