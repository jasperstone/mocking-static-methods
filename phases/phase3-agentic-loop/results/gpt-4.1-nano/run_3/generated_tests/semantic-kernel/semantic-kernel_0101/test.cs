using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.Setup(b => b.Services).Returns(serviceCollection);
            builderMock.Setup(b => b.Build()).Returns(serviceProvider);

            string modelId = "model-id";
            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
            string location = "us-central1";
            string projectId = "project-id";

            // Act
            var result = builderMock.Object.AddVertexAIGeminiChatCompletion(modelId, tokenProvider, location, projectId);

            // Assert
            Assert.Same(builderMock.Object, result);
            var serviceProviderAfter = builderMock.Object.Build();
            var loggerFactory = serviceProviderAfter.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Equal(mockLoggerFactory.Object, loggerFactory);
        }
    }
}
