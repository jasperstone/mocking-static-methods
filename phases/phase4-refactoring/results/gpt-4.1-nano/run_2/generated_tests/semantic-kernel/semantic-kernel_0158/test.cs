using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;

namespace SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_WithServiceProvider_ReturnsService()
        {
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var result = services.AddOllamaTextGeneration("model", new OllamaApiClient());

            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaChatCompletion_WithServiceProvider_ReturnsService()
        {
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var result = services.AddOllamaChatCompletion("model", new Uri("http://localhost"));

            var provider = result.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithHttpClient_ReturnsService()
        {
            var services = new ServiceCollection();
            var httpClient = new HttpClient();
            var result = services.AddOllamaTextGeneration("model", httpClient);
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }
    }
}
