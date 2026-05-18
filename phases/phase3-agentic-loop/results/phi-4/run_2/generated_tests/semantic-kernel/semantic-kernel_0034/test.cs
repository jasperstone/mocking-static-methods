using System;
using System.Net.Http;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_WhenChatClientIsNull_UsesServiceProviderToGetChatClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockChatClient = new Mock<ChatCompletionsClient>().Object;
            var serviceProvider = new MockServiceProvider(mockChatClient);

            // Act
            services.AddAzureAIInferenceChatCompletion("model-id", null, "service-id");

            // Assert
            Assert.Same(mockChatClient, serviceProvider.GetRequiredService<ChatCompletionsClient>());
        }

        private class MockServiceProvider : IServiceProvider
        {
            private readonly object _chatClient;

            public MockServiceProvider(object chatClient)
            {
                _chatClient = chatClient;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ChatCompletionsClient))
                {
                    return _chatClient;
                }

                return null;
            }

            public T GetRequiredService<T>()
            {
                return (T)GetService(typeof(T));
            }

            public object GetRequiredService(Type serviceType)
            {
                var service = GetService(serviceType);
                if (service == null)
                {
                    throw new InvalidOperationException($"Service of type {serviceType} is not registered.");
                }
                return service;
            }
        }
    }
}
