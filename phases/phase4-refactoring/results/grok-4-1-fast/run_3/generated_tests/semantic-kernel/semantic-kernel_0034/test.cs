using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;
using Azure.AI.Inference;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_ChatClientOverload_WithNullChatClient_ResolvesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var chatClient = new Mock<ChatCompletionsClient>().Object;
        services.AddSingleton(chatClient);

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-35-turbo", chatClient: null);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ChatClientOverload_WithNullChatClient_MissingFromServiceProvider_Throws()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddAzureAIInferenceChatCompletion("gpt-35-turbo", chatClient: null);
        var serviceProvider = services.BuildServiceProvider();
        
        Assert.ThrowsAny<Exception>(() => serviceProvider.GetKeyedService<IChatCompletionService>());
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ChatClientOverload_WithProvidedChatClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var chatClient = new Mock<ChatCompletionsClient>().Object;

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-35-turbo", chatClient: chatClient);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ChatClientOverload_WithCustomServiceId_UsesCustomKey()
    {
        // Arrange
        var services = new ServiceCollection();
        var chatClient = new Mock<ChatCompletionsClient>().Object;
        services.AddSingleton(chatClient);

        const string customServiceId = "MyCustomService";

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-35-turbo", chatClient: null, serviceId: customServiceId);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatCompletionService = serviceProvider.GetKeyedService<IChatCompletionService>(customServiceId);
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ChatClientOverload_NullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            ((IServiceCollection)null!).AddAzureAIInferenceChatCompletion("model", chatClient: null));
        Assert.Equal("services", exception.ParamName);
    }
}
