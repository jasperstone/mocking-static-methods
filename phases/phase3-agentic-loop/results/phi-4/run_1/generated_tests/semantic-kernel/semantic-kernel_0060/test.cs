using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel; // Assuming this is the correct namespace for IKernelBuilder
using Microsoft.SemanticKernel.Connectors.AzureOpenAI; // Assuming this is the correct namespace for AzureOpenAIClient and AzureOpenAIAudioToTextService

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_RegistersServiceCorrectly()
        {
            // Arrange
            var builder = new Mock<IKernelBuilder>();
            var serviceProvider = new Mock<IServiceProvider>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var openAIClient = new Mock<AzureOpenAIClient>();

            serviceProvider
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactory.Object);

            serviceProvider
                .Setup(sp => sp.GetRequiredService<AzureOpenAIClient>())
                .Returns(openAIClient.Object);

            builder.Setup(b => b.Services).Returns(new ServiceCollection());

            // Act
            var result = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builder.Object,
                "deploymentName",
                null,
                "serviceId",
                "modelId");

            // Assert
            Assert.Same(builder.Object, result);

            var services = builder.Object.Services;
            var serviceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);

            var factory = (Func<IServiceProvider, object?, IAudioToTextService>)serviceDescriptor.ImplementationFactory;
            var audioToTextService = factory(serviceProvider.Object, null);

            Assert.NotNull(audioToTextService);
            Assert.IsType<AzureOpenAIAudioToTextService>(audioToTextService);

            var azureOpenAIAudioToTextService = (AzureOpenAIAudioToTextService)audioToTextService;
            Assert.Equal("deploymentName", azureOpenAIAudioToTextService.DeploymentName);
            Assert.Same(openAIClient.Object, azureOpenAIAudioToTextService.Client);
            Assert.Equal("modelId", azureOpenAIAudioToTextService.ModelId);
            Assert.Same(loggerFactory.Object, azureOpenAIAudioToTextService.LoggerFactory);
        }
    }
}
