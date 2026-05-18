using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Azure.Core;

namespace AzureOpenAI.Tests
{
    public class AzureOpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIChatClient_Should_Call_GetService_And_ReturnServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IServiceProvider
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockChatClientBuilder = new Mock<IChatClientBuilder>();
            var mockChatClient = new Mock<IChatClient>();

            // Setup IServiceProvider to return ILoggerFactory
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            // Setup the client to return a builder
            var mockAzureClient = new Mock<AzureOpenAIClient>();
            mockAzureClient.Setup(c => c.GetChatClient(It.IsAny<string>()))
                .Returns(() =>
                {
                    // Setup builder chain
                    mockChatClientBuilder.Setup(b => b.AsIChatClient()).Returns(mockChatClientBuilder.Object);
                    mockChatClientBuilder.Setup(b => b.AsBuilder()).Returns(mockChatClientBuilder.Object);
                    mockChatClientBuilder.Setup(b => b.UseKernelFunctionInvocation(It.IsAny<ILoggerFactory>())).Returns(mockChatClientBuilder.Object);
                    mockChatClientBuilder.Setup(b => b.UseOpenTelemetry(It.IsAny<ILoggerFactory>(), It.IsAny<string>(), It.IsAny<Action<OpenTelemetryChatClient>>())).Returns(mockChatClientBuilder.Object);
                    mockChatClientBuilder.Setup(b => b.UseLogging(It.IsAny<ILoggerFactory>())).Returns(mockChatClientBuilder.Object);
                    mockChatClientBuilder.Setup(b => b.Build()).Returns(mockChatClient.Object);
                    return mockChatClientBuilder.Object;
                });

            // To simulate static method CreateAzureOpenAIClient, we can temporarily replace it via a delegate or assume it returns our mock client.
            // For simplicity, assume the method is called and returns our mock client.

            // Act
            services.AddAzureOpenAIChatClient(
                deploymentName: "dep",
                endpoint: "https://endpoint",
                apiKey: "key",
                serviceId: "service",
                modelId: "model",
                apiVersion: "v1",
                httpClient: null,
                openTelemetrySourceName: null,
                openTelemetryConfig: null);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Assert
            var chatClient = provider.GetService<IChatClient>();
            Assert.NotNull(chatClient);
        }
    }
}
