using System;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureAIInference.UnitTests.Extensions
{
    public class AzureAIInferenceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureAIInferenceChatCompletion_ResolvesChatClientFromServiceProviderWhenNoneProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var resolutionAttempts = 0;

            services.AddSingleton<ChatCompletionsClient>(_ =>
            {
                resolutionAttempts++;
                throw new ChatClientResolutionException();
            });

            var serviceId = "test-service-id";
            services.AddAzureAIInferenceChatCompletion("test-model", chatClient: null, serviceId: serviceId);

            using var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<ChatClientResolutionException>(
                () => provider.GetRequiredKeyedService<IChatCompletionService>(serviceId));

            Assert.Equal(1, resolutionAttempts);
        }

        private sealed class ChatClientResolutionException : Exception
        {
        }
    }
}
