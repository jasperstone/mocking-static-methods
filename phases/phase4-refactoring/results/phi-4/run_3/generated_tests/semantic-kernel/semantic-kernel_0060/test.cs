using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_CallsGetServiceAndGetRequiredServiceCorrectly()
        {
            // Arrange
            var builder = new Mock<IKernelBuilder>();
            var serviceProvider = new Mock<IServiceProvider>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var openAIClient = new Mock<AzureOpenAIClient>();

            builder.Setup(b => b.Services).Returns(new ServiceCollection());

            serviceProvider
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactory.Object);

            serviceProvider
                .Setup(sp => sp.GetRequiredService<AzureOpenAIClient>())
                .Returns(openAIClient.Object);

            var deploymentName = "deployment";
            var modelId = "model";

            // Act
            AzureOpenAIKernelBuilderExtensions.AddAzureOpenAIAudioToText(builder.Object, deploymentName, null, null, modelId);

            // Assert
            serviceProvider.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
            serviceProvider.Verify(sp => sp.GetRequiredService<AzureOpenAIClient>(), Times.Once);

            var factory = builder.Object.Services.FirstOrDefault(s => s.ServiceType == typeof(IAudioToTextService))?.ImplementationFactory;
            Assert.NotNull(factory);

            var serviceProviderArg = factory.Method.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(IServiceProvider));
            Assert.NotNull(serviceProviderArg);

            var serviceProviderInstance = serviceProvider.Object;
            var result = factory.Invoke(new object[] { serviceProviderInstance, null });

            Assert.IsType<AzureOpenAIAudioToTextService>(result);
            Assert.Equal(deploymentName, ((AzureOpenAIAudioToTextService)result).DeploymentName);
            Assert.Same(openAIClient.Object, ((AzureOpenAIAudioToTextService)result).Client);
            Assert.Same(loggerFactory.Object, ((AzureOpenAIAudioToTextService)result).LoggerFactory);
        }
    }
}
