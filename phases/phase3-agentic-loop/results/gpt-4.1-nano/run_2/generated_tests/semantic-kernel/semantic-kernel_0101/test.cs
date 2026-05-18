using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google.Extensions;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_Should_Call_GetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock ILoggerFactory
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Create a mock IKernelBuilder
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.Setup(b => b.Services).Returns(serviceCollection);

            // Act
            var result = builderMock.Object.AddVertexAIGeminiChatCompletion(
                modelId: "model",
                bearerTokenProvider: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "project");

            // Assert
            // Build the service provider from the service collection to verify the registration
            var sp = serviceCollection.BuildServiceProvider();

            // Verify that GetService<ILoggerFactory> returns the mocked ILoggerFactory
            var loggerFactory = sp.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactory);
            Assert.Same(loggerFactoryMock.Object, loggerFactory);
        }
    }
}
