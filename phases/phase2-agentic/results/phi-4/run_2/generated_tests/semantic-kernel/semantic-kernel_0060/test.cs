using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_ShouldRegisterServiceWithCorrectParameters()
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

            builder.Object.Services = new ServiceCollection();

            var deploymentName = "deployment";
            var modelId = "model";
            var serviceId = "service";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builder.Object,
                deploymentName,
                null,
                serviceId,
                modelId);

            // Assert
            var serviceDescriptor = builder.Object.Services
                .FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);

            var factory = (Func<IServiceProvider, object?, IAudioToTextService>)serviceDescriptor.ImplementationFactory;
            var service = factory(serviceProvider.Object, null);

            Assert.IsType<AzureOpenAIAudioToTextService>(service);
            var audioToTextService = (AzureOpenAIAudioToTextService)service;

            Assert.Equal(deploymentName, audioToTextService.DeploymentName);
            Assert.Same(openAIClient.Object, audioToTextService.Client);
            Assert.Equal(modelId, audioToTextService.ModelId);
            Assert.Same(loggerFactory.Object, audioToTextService.LoggerFactory);
        }
    }
}
