using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Azure.AI.Inference;
using Azure.Core;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_ServiceProvider_GetRequiredService_ChatCompletionsClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAzureAIInferenceChatCompletion("modelId", new ChatCompletionsClient(new Uri("https://example.com"), new AzureKeyCredential("apiKey")));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ServiceProvider_GetRequiredService_ChatCompletionsClient_NullChatClient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ChatCompletionsClient>(new ChatCompletionsClient(new Uri("https://example.com"), new AzureKeyCredential("apiKey")));
        services.AddAzureAIInferenceChatCompletion("modelId");
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var chatCompletionService = serviceProvider.GetService<IChatCompletionService>();

        // Assert
        Assert.NotNull(chatCompletionService);
    }
}
