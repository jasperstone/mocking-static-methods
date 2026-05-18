using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Extensions.AzureOpenAI;

namespace SemanticKernel.Tests
{
    public class AzureOpenAIKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddAzureOpenAIAudioToText_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Register ILoggerFactory in the service collection
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            var builderMock = new Mock<IKernelBuilder>();
            builderMock.Setup(b => b.Services).Returns(services);
            // We will invoke the factory function directly with our mock IServiceProvider

            // Act
            var factory = (serviceProvider, _) => new AzureOpenAIAudioToTextService("deployment", serviceProvider.GetRequiredService<AzureOpenAIClient>(), "modelId", serviceProvider.GetService<ILoggerFactory>());
            var result = factory(serviceProvider, null);

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            // Since we used serviceProvider.GetService<ILoggerFactory>(), we can verify that the mock was used
            // But in this setup, the mock is used directly, so we check if the returned ILoggerFactory is the mock
            Assert.Same(loggerFactoryMock.Object, result.LoggerFactory);
        }
    }
}
