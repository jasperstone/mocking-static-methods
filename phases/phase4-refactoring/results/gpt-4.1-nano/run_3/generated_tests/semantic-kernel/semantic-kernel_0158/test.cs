using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;

// Minimal stub for OllamaApiClient to avoid compilation errors
public class OllamaApiClient
{
    public OllamaApiClient() { }
    public OllamaApiClient(Uri endpoint, string modelId) { }
    public OllamaApiClient(HttpClient httpClient, string modelId) { }
}

// Minimal stub for IChatCompletionService to avoid compilation errors
public interface IChatCompletionService { }

namespace SemanticKernel.Tests
{
    public class OllamaServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOllamaTextGeneration_WithOllamaClient_SetsUpServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOllamaClient = new Mock<OllamaApiClient>();
            services.AddSingleton(mockOllamaClient.Object);
            services.AddLogging();

            // Act
            var result = services.AddOllamaTextGeneration("testModel", mockOllamaClient.Object);

            // Assert
            Assert.Contains(result, s => s.ServiceType == typeof(ITextGenerationService));
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithHttpClient_SetsUpServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var httpClient = new HttpClient();

            // Act
            var result = services.AddOllamaTextGeneration("modelId", httpClient);

            // Assert
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithOllamaApiClient_SetsUpServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var ollamaClient = new OllamaApiClient();

            // Act
            var result = services.AddOllamaTextGeneration("modelId", ollamaClient);

            // Assert
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaTextGeneration_WithServiceProvider_OllamaClientIsResolved()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<OllamaApiClient>();
            services.AddTransient<IServiceProvider>(sp => sp);

            // Act
            var result = services.AddOllamaTextGeneration();

            // Assert
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<ITextGenerationService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddOllamaChatCompletion_BuildsServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            var result = services.AddOllamaChatCompletion("modelId", new Uri("http://localhost"));

            // Assert
            var provider = result.BuildServiceProvider();
            var service = provider.GetService<IChatCompletionService>();
            Assert.NotNull(service);
        }
    }
}
