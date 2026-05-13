using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedClientAndLoggerFactory()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(serviceCollection);

            var deploymentName = "deployment1";
            var modelId = "model1";
            var serviceId = "service1";

            var openAIClient = new AzureOpenAIClient(new Uri("https://fakeuri"), new Azure.Core.AzureKeyCredential("key"));
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add ILoggerFactory to service collection to be resolved
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            var builder = builderMock.Object;

            // Act
            var returnedBuilder = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builder,
                deploymentName,
                openAIClient,
                serviceId,
                modelId);

            // Assert
            Assert.Same(builder, returnedBuilder);

            // Resolve the registered IAudioToTextService factory and invoke it to test the factory logic
            var provider = serviceCollection.BuildServiceProvider();

            var audioToTextService = provider.GetRequiredService<IAudioToTextService>();

            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);

            var azureService = (AzureOpenAIAudioToTextService)audioToTextService;

            // The AzureOpenAIAudioToTextService should have the deploymentName and modelId set correctly
            Assert.Equal(deploymentName, azureService.DeploymentName);
            Assert.Equal(modelId, azureService.ModelId);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ResolvesClientFromServiceProvider_WhenClientIsNull()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(serviceCollection);

            var deploymentName = "deployment2";
            var modelId = "model2";
            var serviceId = "service2";

            var openAIClient = new AzureOpenAIClient(new Uri("https://fakeuri2"), new Azure.Core.AzureKeyCredential("key2"));
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add AzureOpenAIClient and ILoggerFactory to service collection to be resolved
            serviceCollection.AddSingleton(openAIClient);
            serviceCollection.AddSingleton(loggerFactoryMock.Object);

            var builder = builderMock.Object;

            // Act
            var returnedBuilder = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builder,
                deploymentName,
                null,
                serviceId,
                modelId);

            // Assert
            Assert.Same(builder, returnedBuilder);

            // Resolve the registered IAudioToTextService factory and invoke it to test the factory logic
            var provider = serviceCollection.BuildServiceProvider();

            var audioToTextService = provider.GetRequiredService<IAudioToTextService>();

            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);

            var azureService = (AzureOpenAIAudioToTextService)audioToTextService;

            // The AzureOpenAIAudioToTextService should have the deploymentName and modelId set correctly
            Assert.Equal(deploymentName, azureService.DeploymentName);
            Assert.Equal(modelId, azureService.ModelId);
        }
    }
}
