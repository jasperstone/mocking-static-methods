using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Http;

namespace SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_ShouldRegisterService_WithLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var builder = new KernelBuilder(serviceProvider);

            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");

            // Act
            var resultBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder,
                "modelId",
                tokenProvider,
                "us-central1",
                "project123");

            // Assert
            Assert.Same(builder, resultBuilder);
            var serviceProviderFinal = resultBuilder.Services.BuildServiceProvider();
            var chatService = serviceProviderFinal.GetService<IChatCompletionService>();
            Assert.NotNull(chatService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldRegisterService_WithLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var builder = new KernelBuilder(serviceProvider);

            // Act
            var resultBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builder,
                "modelId",
                "bearerToken",
                "us-central1",
                "project123");

            // Assert
            Assert.Same(builder, resultBuilder);
            var serviceProviderFinal = resultBuilder.Services.BuildServiceProvider();
            var chatService = serviceProviderFinal.GetService<IChatCompletionService>();
            Assert.NotNull(chatService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatService);
        }
    }

    // Minimal implementation of IKernelBuilder for testing
    public class KernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; }

        public KernelBuilder(IServiceProvider serviceProvider)
        {
            Services = new ServiceCollection();
            // Optionally, add existing services from the provider
        }
    }
}
