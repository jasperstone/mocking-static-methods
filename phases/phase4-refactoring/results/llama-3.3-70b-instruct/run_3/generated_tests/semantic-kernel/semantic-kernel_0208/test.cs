using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Moq;
using Microsoft.SemanticKernel;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAIChatClient_AddsServiceToList()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var orgId = "orgId";
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<Microsoft.SemanticKernel.IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_ThrowsExceptionWhenServicesIsNull()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null).AddOpenAIChatClient("modelId", new Uri("https://example.com"), "apiKey"));
        }

        [Fact]
        public void AddOpenAIChatClient_ThrowsExceptionWhenModelIdIsNull()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddOpenAIChatClient(null, new Uri("https://example.com"), "apiKey"));
        }

        [Fact]
        public void AddOpenAIChatClient_UsesProvidedHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var orgId = "orgId";
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<Microsoft.SemanticKernel.IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_UsesDefaultHttpClientWhenNoHttpClientIsProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var orgId = "orgId";
            var serviceId = "serviceId";

            // Act
            services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<Microsoft.SemanticKernel.IChatClient>();
            Assert.NotNull(chatClient);
        }

        [Fact]
        public void AddOpenAIChatClient_SetsBaseAddressOfHttpClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var modelId = "modelId";
            var endpoint = new Uri("https://example.com");
            var apiKey = "apiKey";
            var orgId = "orgId";
            var serviceId = "serviceId";
            var httpClient = new HttpClient();

            // Act
            services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId, httpClient);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var chatClient = serviceProvider.GetService<Microsoft.SemanticKernel.IChatClient>();
            Assert.NotNull(chatClient);
            Assert.Equal(endpoint, httpClient.BaseAddress);
        }
    }
}
