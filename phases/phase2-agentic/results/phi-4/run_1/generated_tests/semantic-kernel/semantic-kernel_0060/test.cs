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
        public void AddAzureOpenAIAudioToText_UsesServiceProviderToGetLoggerFactory()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(new ServiceCollection());

            var deploymentName = "deployment";
            var modelId = "model";
            var serviceId = "service";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(
                builderMock.Object,
                deploymentName,
                null,
                serviceId,
                modelId);

            // Assert
            var serviceDescriptor = builderMock.Object.Services
                .FirstOrDefault(sd => sd.ServiceType == typeof(IAudioToTextService) && sd.ImplementationFactory != null);

            Assert.NotNull(serviceDescriptor);

            var serviceProvider = serviceProviderMock.Object;
            var factory = (Func<IServiceProvider, object?, IAudioToTextService>)serviceDescriptor.ImplementationFactory;
            var service = factory(serviceProvider, null);

            Assert.IsType<AzureOpenAIAudioToTextService>(service);
            Assert.Equal(deploymentName, ((AzureOpenAIAudioToTextService)service).DeploymentName);
            Assert.Equal(modelId, ((AzureOpenAIAudioToTextService)service).ModelId);
            Assert.Same(loggerFactoryMock.Object, ((AzureOpenAIAudioToTextService)service).LoggerFactory);
        }
    }
}
