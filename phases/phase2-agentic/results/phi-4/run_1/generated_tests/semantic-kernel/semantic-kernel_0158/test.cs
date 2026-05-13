using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WhenOllamaApiClientIsProvided_ShouldReturnUpdatedServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var ollamaClient = new Mock<OllamaApiClient>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService<OllamaApiClient>())
                .Returns(ollamaClient.Object);

            // Act
            var updatedServices = services.AddOllamaTextEmbeddingGeneration(serviceProvider: serviceProviderMock.Object);

            // Assert
            var serviceDescriptor = updatedServices.FirstOrDefault(sd => sd.ServiceType == typeof(ITextEmbeddingGenerationService));
            Assert.NotNull(serviceDescriptor);
            Assert.IsType<KeyedSingletonServiceDescriptor>(serviceDescriptor);
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_WhenNoOllamaApiClientIsFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService<OllamaApiClient>())
                .Returns((OllamaApiClient)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddOllamaTextEmbeddingGeneration(serviceProvider: serviceProviderMock.Object));
        }
    }
}
