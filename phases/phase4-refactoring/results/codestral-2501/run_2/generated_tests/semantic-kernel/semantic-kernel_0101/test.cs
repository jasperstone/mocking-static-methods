using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Google.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class VertexAIKernelBuilderExtensionsTests
    {
        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithBearerTokenProvider_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithBearerKey_ShouldAddService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            var modelId = "test-model";
            var bearerKey = "test-key";
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act
            kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerKey, location, projectId, apiVersion, serviceId, httpClient);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IChatCompletionService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<VertexAIGeminiChatCompletionService>(service);
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithNullBuilder_ShouldThrowArgumentNullException()
        {
            // Arrange
            IKernelBuilder builder = null;
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithNullModelId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            string modelId = null;
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithNullBearerTokenProvider_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            var modelId = "test-model";
            Func<ValueTask<string>> bearerTokenProvider = null;
            var location = "test-location";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithNullLocation_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            string location = null;
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }

        [Fact]
        public async Task AddVertexAIGeminiChatCompletion_WithNullProjectId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new KernelBuilder(serviceCollection);
            var modelId = "test-model";
            var bearerTokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("test-token"));
            var location = "test-location";
            string projectId = null;
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";
            var httpClient = new HttpClient();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => kernelBuilder.AddVertexAIGeminiChatCompletion(modelId, bearerTokenProvider, location, projectId, apiVersion, serviceId, httpClient));
        }
    }
}
