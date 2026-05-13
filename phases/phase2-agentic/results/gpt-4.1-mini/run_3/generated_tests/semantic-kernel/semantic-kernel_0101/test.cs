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

namespace Microsoft.SemanticKernel.Tests.Extensions
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        private class TestKernelBuilder : IKernelBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();

            public IKernelBuilder ConfigureServices(Action<IServiceCollection> configure)
            {
                configure(Services);
                return this;
            }
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var builder = new TestKernelBuilder();
            string modelId = "test-model";
            string location = "us-central1";
            string projectId = "test-project";
            VertexAIVersion apiVersion = VertexAIVersion.V1;
            string? serviceId = "test-service";

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup a service provider that returns the logger factory when requested
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Setup a service provider factory to inject our mock service provider
            builder.Services.AddSingleton(serviceProviderMock.Object);

            // Bearer token provider returns a dummy token
            Func<ValueTask<string>> bearerTokenProvider = () => new ValueTask<string>("dummy-token");

            // Act
            builder.AddVertexAIGeminiChatCompletion(
                modelId,
                bearerTokenProvider,
                location,
                projectId,
                apiVersion,
                serviceId,
                null);

            // Build the service provider from the service collection
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Retrieve the registered IChatCompletionService using the serviceId key
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);

            // Verify that the service provider's GetService was called for ILoggerFactory
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var builder = new TestKernelBuilder();
            string modelId = "test-model";
            string bearerKey = "test-bearer-key";
            string location = "us-central1";
            string projectId = "test-project";
            VertexAIVersion apiVersion = VertexAIVersion.V1;
            string? serviceId = "test-service";

            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup a service provider that returns the logger factory when requested
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            builder.Services.AddSingleton(serviceProviderMock.Object);

            // Act
            builder.AddVertexAIGeminiChatCompletion(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                null);

            // Build the service provider from the service collection
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Retrieve the registered IChatCompletionService using the serviceId key
            var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(chatCompletionService);
            Assert.IsType<VertexAIGeminiChatCompletionService>(chatCompletionService);

            // Verify that the service provider's GetService was called for ILoggerFactory
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
