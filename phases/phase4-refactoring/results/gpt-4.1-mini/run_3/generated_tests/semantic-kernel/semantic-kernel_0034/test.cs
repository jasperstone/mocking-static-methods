using System;
using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Xunit;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_UsesServiceProviderGetRequiredServiceWhenChatClientNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Create a real ChatCompletionsClient instance with minimal constructor args
        var endpoint = new Uri("http://localhost");
        var credential = new AzureKeyCredential("dummy");
        var options = new AzureAIInferenceClientOptions();

        var chatClient = new ChatCompletionsClient(endpoint, credential, options);

        // Register ChatCompletionsClient in DI so GetRequiredService can find it
        services.AddSingleton(chatClient);

        // Act
        services.AddAzureAIInferenceChatCompletion("modelId", chatClient: null);
        var provider = services.BuildServiceProvider();

        // Resolve the IChatCompletionService, which triggers the factory and calls GetRequiredService on IServiceProvider
        var chatCompletionService = provider.GetRequiredService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_UsesProvidedChatClient()
    {
        // Arrange
        var services = new ServiceCollection();

        var endpoint = new Uri("http://localhost");
        var credential = new AzureKeyCredential("dummy");
        var options = new AzureAIInferenceClientOptions();

        var chatClient = new ChatCompletionsClient(endpoint, credential, options);

        // Act
        services.AddAzureAIInferenceChatCompletion("modelId", chatClient);
        var provider = services.BuildServiceProvider();
        var chatCompletionService = provider.GetRequiredService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);
    }
}
