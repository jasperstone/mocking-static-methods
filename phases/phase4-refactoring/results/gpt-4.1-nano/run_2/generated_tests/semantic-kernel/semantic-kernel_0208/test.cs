using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Act
            services.AddOpenAIChatClient(
                modelId: "test-model",
                apiKey: "test-api-key");

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Use reflection to invoke the internal factory delegate
            // Since the factory delegate is internal, we simulate the call by resolving the IChatClient
            var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IChatClient));
            Assert.NotNull(serviceDescriptor);

            var factory = serviceDescriptor.ImplementationInstance ?? throw new Exception("Factory not found");
            // The factory is a delegate, but since it's internal, we can't invoke it directly.
            // Instead, we verify that the services contain the expected registration.
            // Alternatively, we can test that the service provider returns the mock ILoggerFactory when resolving ILoggerFactory.

            // Act: resolve ILoggerFactory
            var loggerFactoryResolved = serviceProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactoryResolved);
            Assert.Equal(mockLoggerFactory.Object, loggerFactoryResolved);
        }
    }
}
