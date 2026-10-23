using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Azure.AI.OpenAI;
using Azure.Core;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.AzureOpenAI.Tests.Extensions
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
            var deploymentName = "testDeployment";
            var modelId = "testModel";
            var serviceId = "testService";

            var openAIClient = new AzureOpenAIClient(new Uri("https://test.endpoint"), new AzureKeyCredential("fakekey"));
            var loggerFactory = LoggerFactory.Create(builder => { });

            // Register the AzureOpenAIClient and ILoggerFactory in the service provider
            builder.Services.AddSingleton(openAIClient);
            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient, serviceId, modelId);

            // Build the service provider to resolve services
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Resolve the IAudioToTextService registered with the serviceId
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesServiceProviderClientWhenClientNotProvided()
        {
            // Arrange
            var builder = new TestKernelBuilder();
            var deploymentName = "testDeployment";
            var modelId = "testModel";
            var serviceId = "testService";

            var openAIClient = new AzureOpenAIClient(new Uri("https://test.endpoint"), new AzureKeyCredential("fakekey"));
            var loggerFactory = LoggerFactory.Create(builder => { });

            // Register the AzureOpenAIClient and ILoggerFactory in the service provider
            builder.Services.AddSingleton(openAIClient);
            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

            // Act
            builder.AddAzureOpenAIAudioToText(deploymentName, null, serviceId, modelId);

            // Build the service provider to resolve services
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Resolve the IAudioToTextService registered with the serviceId
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }
    }
}
