using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests.Connectors.Google.Extensions
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<IKernelBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            var modelId = "test-model";
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("token");
            var location = "us-central1";
            var projectId = "test-project";

            // Act
            var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builderMock.Object,
                modelId,
                bearerTokenProvider,
                location,
                projectId);

            // Assert
            Assert.Same(builderMock.Object, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the registered IChatCompletionService
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);

            // The loggerFactory should be resolved via IServiceProvider.GetService<ILoggerFactory>
            var loggerFactoryResolved = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactoryResolved);
            Assert.Same(loggerFactoryMock.Object, loggerFactoryResolved);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<IKernelBuilder>();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "us-central1";
            var projectId = "test-project";

            // Act
            var returnedBuilder = VertexAIKernelBuilderExtensions.AddVertexAIGeminiChatCompletion(
                builderMock.Object,
                modelId,
                bearerKey,
                location,
                projectId);

            // Assert
            Assert.Same(builderMock.Object, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the registered IChatCompletionService
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();
            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);

            // The loggerFactory should be resolved via IServiceProvider.GetService<ILoggerFactory>
            var loggerFactoryResolved = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(loggerFactoryResolved);
            Assert.Same(loggerFactoryMock.Object, loggerFactoryResolved);
        }
    }
}
