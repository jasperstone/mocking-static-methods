using System;
using System.Linq;
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
            var builder = new Mock<IKernelBuilder>();
            builder.Setup(b => b.Services).Returns(serviceCollection);

            var deploymentName = "testDeployment";
            var modelId = "testModel";
            var serviceId = "testService";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockAzureOpenAIClient = new Mock<AzureOpenAIClient>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(mockAzureOpenAIClient.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builder.Object, deploymentName, null, serviceId, modelId);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IAudioToTextService) && descriptor.ServiceKey == serviceId);
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);

            var factory = (Func<IServiceProvider, object?, AzureOpenAIAudioToTextService>)serviceDescriptor.ImplementationFactory;
            var audioToTextService = factory(mockServiceProvider.Object, null);

            Assert.NotNull(audioToTextService);
            Assert.Equal(deploymentName, audioToTextService.DeploymentName);
            Assert.Equal(modelId, audioToTextService.ModelId);
            Assert.Same(mockAzureOpenAIClient.Object, audioToTextService.Client);
            Assert.Same(mockLoggerFactory.Object, audioToTextService.LoggerFactory);
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldUseProvidedAzureOpenAIClient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var builder = new Mock<IKernelBuilder>();
            builder.Setup(b => b.Services).Returns(serviceCollection);

            var deploymentName = "testDeployment";
            var modelId = "testModel";
            var serviceId = "testService";
            var providedClient = new Mock<AzureOpenAIClient>().Object;

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builder.Object, deploymentName, providedClient, serviceId, modelId);

            // Assert
            var serviceDescriptor = serviceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IAudioToTextService) && descriptor.ServiceKey == serviceId);
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);

            var factory = (Func<IServiceProvider, object?, AzureOpenAIAudioToTextService>)serviceDescriptor.ImplementationFactory;
            var audioToTextService = factory(mockServiceProvider.Object, null);

            Assert.NotNull(audioToTextService);
            Assert.Equal(deploymentName, audioToTextService.DeploymentName);
            Assert.Equal(modelId, audioToTextService.ModelId);
            Assert.Same(providedClient, audioToTextService.Client);
            Assert.Same(mockLoggerFactory.Object, audioToTextService.LoggerFactory);
        }
    }
}
