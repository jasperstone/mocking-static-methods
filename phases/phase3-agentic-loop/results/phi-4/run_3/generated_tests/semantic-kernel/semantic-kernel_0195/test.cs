using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(
                services,
                "text-embedding-3",
                null,
                null,
                null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
