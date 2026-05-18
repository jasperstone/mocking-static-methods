using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Azure.AI.OpenAI;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedAzureOpenAIClient()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var services = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var deploymentName = "deployment";
            var modelId = "model";

            var azureOpenAIClientMock = new Mock<AzureOpenAIClient>(new Uri("http://localhost"), null!, null!);

            // Act
            var builder = builderMock.Object;
            builder.AddAzureOpenAIAudioToText(deploymentName, azureOpenAIClientMock.Object, serviceId: null, modelId: modelId);

            // Build service provider from services to resolve the factory
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IAudioToTextService from the keyed singleton registration
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesServiceProviderToGetAzureOpenAIClientAndLoggerFactory()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var services = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(services);

            var deploymentName = "deployment";
            var modelId = "model";

            var azureOpenAIClientMock = new Mock<AzureOpenAIClient>(new Uri("http://localhost"), null!, null!);
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add AzureOpenAIClient and ILoggerFactory to services so they can be resolved by the factory
            services.AddSingleton(azureOpenAIClientMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            var builder = builderMock.Object;
            builder.AddAzureOpenAIAudioToText(deploymentName, openAIClient: null, serviceId: null, modelId: modelId);

            // Build service provider from services to resolve the factory
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IAudioToTextService from the keyed singleton registration
            var audioToTextService = serviceProvider.GetService<IAudioToTextService>();

            // Assert
            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);
        }
    }
}
