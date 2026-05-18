using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions
{
    public class OpenAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesServiceProviderGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOpenAIClient = new Mock<OpenAIClient>(MockBehavior.Strict);

            // We cannot mock extension methods like GetRequiredService, so we will not setup it.
            // Instead, we will test that GetService<ILoggerFactory> is called by using a real service provider with a spy.

            // Add the OpenAIClient to the services so the factory does not call GetRequiredService
            services.AddSingleton(mockOpenAIClient.Object);

            // Add a spy logger factory to verify it is requested
            var loggerFactoryCalled = false;
            services.AddSingleton<ILoggerFactory>(sp =>
            {
                loggerFactoryCalled = true;
                return mockLoggerFactory.Object;
            });

            // Act
            var result = services.AddOpenAITextEmbeddingGeneration(
                modelId: "test-model",
                openAIClient: mockOpenAIClient.Object,
                serviceId: "test-service",
                dimensions: 123);

            // Build the service provider to invoke the factory delegate
            var sp = services.BuildServiceProvider();

            // Resolve the keyed singleton service to trigger the factory delegate
            var embeddingService = sp.GetService(typeof(ITextEmbeddingGenerationService));

            // Assert
            Assert.NotNull(embeddingService);
            Assert.True(loggerFactoryCalled, "ILoggerFactory was not requested from the service provider.");
        }
    }
}
