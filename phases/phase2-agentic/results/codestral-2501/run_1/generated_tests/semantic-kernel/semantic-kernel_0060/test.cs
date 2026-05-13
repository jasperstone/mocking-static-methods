using System;
using System.Net.Http;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldAddAudioToTextService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var deploymentName = "testDeployment";
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(kernelBuilder.Object, deploymentName, serviceId: serviceId, modelId: modelId);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(serviceId, serviceDescriptor.ServiceKey);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldUseProvidedOpenAIClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var deploymentName = "testDeployment";
            var openAIClient = new Mock<AzureOpenAIClient>(new Uri("https://example.com"), new ApiKeyCredential("testKey"));
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(kernelBuilder.Object, deploymentName, openAIClient.Object, serviceId, modelId);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(serviceId, serviceDescriptor.ServiceKey);

            var factory = serviceDescriptor.ImplementationFactory;
            var serviceProvider = new Mock<IServiceProvider>();
            var audioToTextService = factory(serviceProvider.Object, null) as AzureOpenAIAudioToTextService;
            Assert.NotNull(audioToTextService);
            Assert.Same(openAIClient.Object, audioToTextService.Client);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldGetOpenAIClientFromServiceProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var kernelBuilder = new Mock<IKernelBuilder>();
            kernelBuilder.Setup(kb => kb.Services).Returns(serviceCollection);

            var deploymentName = "testDeployment";
            var serviceId = "testServiceId";
            var modelId = "testModelId";

            var openAIClient = new Mock<AzureOpenAIClient>(new Uri("https://example.com"), new ApiKeyCredential("testKey"));
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(openAIClient.Object);

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(kernelBuilder.Object, deploymentName, serviceId: serviceId, modelId: modelId);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.Lifetime == ServiceLifetime.Singleton);
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(serviceId, serviceDescriptor.ServiceKey);

            var factory = serviceDescriptor.ImplementationFactory;
            var audioToTextService = factory(serviceProvider.Object, null) as AzureOpenAIAudioToTextService;
            Assert.NotNull(audioToTextService);
            Assert.Same(openAIClient.Object, audioToTextService.Client);
        }
    }
}
