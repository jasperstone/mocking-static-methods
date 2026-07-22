using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Collections.Generic;

namespace SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVertexAIGeminiChatCompletion_ShouldRegisterService_WithLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new KernelBuilder(services);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

            string modelId = "model123";
            string location = "us-central1";
            string projectId = "proj-abc";

            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");

            // Act
            builder.AddVertexAIGeminiChatCompletion(modelId, tokenProvider, location, projectId);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldRegisterService_WithLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new KernelBuilder(services);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

            string modelId = "model123";
            string bearerKey = "abc123";
            string location = "us-central1";
            string projectId = "proj-abc";

            // Act
            builder.AddVertexAIGeminiChatCompletion(modelId, bearerKey, location, projectId);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }

        [Fact]
        public void AddVertexAIGeminiChatCompletion_ShouldUseHttpClientAndLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new KernelBuilder(services);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);
            var httpClient = new HttpClient();

            string modelId = "model123";
            string location = "us-central1";
            string projectId = "proj-abc";

            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");

            // Act
            builder.AddVertexAIGeminiChatCompletion(modelId, tokenProvider, location, projectId, httpClient: httpClient);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }
    }

    // Minimal KernelBuilder implementation for testing
    public class KernelBuilder : IKernelBuilder
    {
        public IServiceCollection Services { get; }

        public KernelBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
