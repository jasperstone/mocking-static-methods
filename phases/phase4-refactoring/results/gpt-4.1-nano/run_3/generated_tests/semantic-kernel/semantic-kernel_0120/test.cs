using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.ChatCompletion;
using System;

namespace SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextGeneration_ShouldRegisterServiceAndResolveILoggerFactory()
        {
            var services = new ServiceCollection();
            var loggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactory.Object);

            var result = services.AddHuggingFaceTextGeneration(
                model: "test-model",
                endpoint: new Uri("https://test-endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: new HttpClient());

            var provider = result.BuildServiceProvider();

            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextGenerationService>(service);

            var logger = provider.GetService<ILoggerFactory>();
            Assert.NotNull(logger);
        }

        [Fact]
        public void AddHuggingFaceChatCompletion_ShouldRegisterServiceAndResolveILoggerFactory()
        {
            var services = new ServiceCollection();
            var loggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactory.Object);

            var result = services.AddHuggingFaceChatCompletion(
                model: "test-model",
                endpoint: new Uri("https://test-endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: new HttpClient());

            var provider = result.BuildServiceProvider();

            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceChatCompletionService>(service);

            var logger = provider.GetService<ILoggerFactory>();
            Assert.NotNull(logger);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldRegisterServiceAndResolveILoggerFactory()
        {
            var services = new ServiceCollection();
            var loggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactory.Object);

            var result = services.AddHuggingFaceTextEmbeddingGeneration(
                model: "test-model",
                endpoint: new Uri("https://test-endpoint"),
                apiKey: "test-api-key",
                serviceId: "test-service",
                httpClient: new HttpClient());

            var provider = result.BuildServiceProvider();

            var service = provider.GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);

            var logger = provider.GetService<ILoggerFactory>();
            Assert.NotNull(logger);
        }
    }
}
