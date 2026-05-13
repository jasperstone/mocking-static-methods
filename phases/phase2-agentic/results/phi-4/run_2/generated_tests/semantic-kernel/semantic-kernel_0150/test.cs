using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            var modelId = "test-model";
            var endpoint = new Uri("http://localhost:1234");

            // Act
            services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddOllamaChatCompletion_ReturnsUpdatedServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "test-model";
            var endpoint = new Uri("http://localhost:1234");

            // Act
            var updatedServices = services.AddOllamaChatCompletion(modelId, endpoint);

            // Assert
            Assert.Same(services, updatedServices);
            Assert.Contains(typeof(IChatCompletionService), services.Select(x => x.ServiceType));
        }
    }
}
