using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Xunit;
using Azure;
using Azure.AI.OpenAI;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        private class TestKernelBuilderPlugins : IKernelBuilderPlugins
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
        }

        private class TestKernelBuilder : IKernelBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();
            public IKernelBuilderPlugins Plugins { get; } = new TestKernelBuilderPlugins();
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedClientAndLoggerFactory()
        {
            // Arrange
            var builder = new TestKernelBuilder();
            string deploymentName = "testDeployment";
            var mockClient = new AzureOpenAIClient(new Uri("https://test.endpoint"), new AzureKeyCredential("fakekey"), new AzureOpenAIClientOptions());
            string modelId = "testModel";

            // Register a dummy ILoggerFactory instance
            var loggerFactory = new LoggerFactory();
            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, mockClient, serviceId: "testService", modelId: modelId);

            // Assert
            var provider = builder.Services.BuildServiceProvider();
            var audioToTextService = provider.GetService<IAudioToTextService>();
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesServiceProviderClientWhenClientIsNull()
        {
            // Arrange
            var builder = new TestKernelBuilder();
            string deploymentName = "testDeployment";
            string modelId = "testModel";

            // Register AzureOpenAIClient in the service collection to be resolved by the factory
            var mockClient = new AzureOpenAIClient(new Uri("https://test.endpoint"), new AzureKeyCredential("fakekey"), new AzureOpenAIClientOptions());
            builder.Services.AddSingleton(mockClient);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient: null, serviceId: "testService", modelId: modelId);

            // Assert
            var provider = builder.Services.BuildServiceProvider();
            var audioToTextService = provider.GetService<IAudioToTextService>();
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }
    }
}
