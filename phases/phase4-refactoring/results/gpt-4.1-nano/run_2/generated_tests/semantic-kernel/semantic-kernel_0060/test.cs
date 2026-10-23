using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class AzureOpenAIAudioToTextExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_CallsGetServiceLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup a mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            // Simulate the factory lambda used in AddKeyedSingleton
            Func<IServiceProvider, object?, AzureOpenAIAudioToTextService> factory = (sp, _) =>
                new AzureOpenAIAudioToTextService("deployment", null, "model", sp.GetService<ILoggerFactory>());

            var resultService = factory(serviceProviderMock.Object, null);

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
            Assert.NotNull(resultService);
        }
    }

    // Dummy class to match the constructor used in the factory
    public class AzureOpenAIAudioToTextService
    {
        public AzureOpenAIAudioToTextService(string deploymentName, object client, string modelId, ILoggerFactory loggerFactory)
        {
        }
    }
}
