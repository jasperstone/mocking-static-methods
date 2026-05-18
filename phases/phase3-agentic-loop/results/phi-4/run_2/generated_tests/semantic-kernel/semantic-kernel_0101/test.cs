using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup the GetService call to return the logger factory
            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Setup the Services property to return the service collection
            builderMock
                .Setup(b => b.Services)
                .Returns(serviceCollectionMock.Object);

            // Setup the BuildServiceProvider to return the mocked service provider
            serviceCollectionMock
                .Setup(sc => sc.BuildServiceProvider())
                .Returns(serviceProviderMock.Object);

            // Act
            VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builderMock.Object,
                "modelId",
                async () => await Task.FromResult("bearerToken"),
                "location",
                "projectId");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
