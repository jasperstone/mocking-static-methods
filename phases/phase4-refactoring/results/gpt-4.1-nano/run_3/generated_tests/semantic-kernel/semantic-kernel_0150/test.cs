using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel; // Correct namespace for extension methods

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_Should_Call_GetService_LoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceProvider = serviceProviderMock.Object;

            // Setup the service provider to return the mock ILoggerFactory
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddOllamaChatCompletion(
                modelId: "test-model",
                endpoint: new Uri("http://localhost"),
                serviceId: "test-service");

            // Build the service provider to resolve the registered service
            var provider = services.BuildServiceProvider();

            // Resolve the IChatCompletionService to trigger the factory
            var chatService = provider.GetService<IChatCompletionService>();

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            Assert.NotNull(chatService);
        }
    }
}
