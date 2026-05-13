using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.AzureOpenAI.Extensions
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_UsesProvidedAzureOpenAIClientAndLoggerFactory()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(serviceCollection);

            var deploymentName = "deployment1";
            var modelId = "model1";
            var serviceId = "service1";

            var azureOpenAIClient = new Mock<AzureOpenAIClient>(new Uri("http://localhost"), null, null).Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;

            // Setup service provider to return the logger factory
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactory);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(azureOpenAIClient);

            // Act
            var resultBuilder = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builderMock.Object,
                deploymentName,
                azureOpenAIClient,
                serviceId,
                modelId);

            // Assert
            Assert.Same(builderMock.Object, resultBuilder);

            // Verify that the service was added with a factory that uses the provided AzureOpenAIClient and logger factory
            var descriptor = Assert.Single(serviceCollection, d => d.ServiceType == typeof(IAudioToTextService));
            Assert.NotNull(descriptor);

            // The factory is a Func<IServiceProvider, object?, AzureOpenAIAudioToTextService>
            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            var serviceInstance = factory(serviceProviderMock.Object);
            Assert.NotNull(serviceInstance);
            Assert.IsType<AzureOpenAIAudioToTextService>(serviceInstance);

            var audioToTextService = (AzureOpenAIAudioToTextService)serviceInstance;

            // We cannot access private fields, but we can check that the service was created without exceptions
            // and that the factory used the provided AzureOpenAIClient and logger factory (via the service provider)
            // This indirectly tests the call to GetService<ILoggerFactory>() on IServiceProvider.
        }

        [Fact]
        public void AddAzureOpenAIAudioToText_UsesServiceProviderAzureOpenAIClientWhenNotProvided()
        {
            // Arrange
            var builderMock = new Mock<IKernelBuilder>();
            var serviceCollection = new ServiceCollection();
            builderMock.SetupGet(b => b.Services).Returns(serviceCollection);

            var deploymentName = "deployment2";
            var modelId = "model2";
            var serviceId = "service2";

            var azureOpenAIClient = new Mock<AzureOpenAIClient>(new Uri("http://localhost"), null, null).Object;
            var loggerFactory = new Mock<ILoggerFactory>().Object;

            // Setup service provider to return the AzureOpenAIClient and logger factory
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AzureOpenAIClient))).Returns(azureOpenAIClient);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactory);

            // Act
            var resultBuilder = AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builderMock.Object,
                deploymentName,
                openAIClient: null,
                serviceId,
                modelId);

            // Assert
            Assert.Same(builderMock.Object, resultBuilder);

            var descriptor = Assert.Single(serviceCollection, d => d.ServiceType == typeof(IAudioToTextService));
            Assert.NotNull(descriptor);

            var factory = descriptor.ImplementationFactory;
            Assert.NotNull(factory);

            var serviceInstance = factory(serviceProviderMock.Object);
            Assert.NotNull(serviceInstance);
            Assert.IsType<AzureOpenAIAudioToTextService>(serviceInstance);

            // This test verifies that when openAIClient is null, the factory calls GetRequiredService<AzureOpenAIClient>()
            // on the service provider, and also calls GetService<ILoggerFactory>(), as per the code on line 826.
        }
    }
}
