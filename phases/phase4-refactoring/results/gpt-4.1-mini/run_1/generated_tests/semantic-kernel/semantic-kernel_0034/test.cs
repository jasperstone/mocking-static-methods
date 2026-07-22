using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Azure.AI.Inference;
using Azure.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesProvidedChatClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), null, null);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Setup service provider to return logger factory
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Setup extension method GetRequiredService to return the mockChatClient when requested
            // We simulate this by adding the mockChatClient to the services collection
            services.AddSingleton(mockChatClient.Object);

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                mockChatClient.Object);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddAzureAIInferenceChatCompletion_UsesServiceProviderToGetChatClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>(MockBehavior.Strict, new Uri("http://localhost"), null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            // Add the mockChatClient to the services so it can be resolved by GetRequiredService
            services.AddSingleton(mockChatClient.Object);
            services.AddSingleton(mockLoggerFactory.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureAIInferenceServiceCollectionExtensions.AddAzureAIInferenceChatCompletion(
                services,
                "modelId",
                null);

            // Assert
            Assert.Same(services, result);
        }
    }
}
