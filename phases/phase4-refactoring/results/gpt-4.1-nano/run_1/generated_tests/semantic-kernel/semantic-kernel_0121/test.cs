using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_Should_Call_GetService_ILoggerFactory()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Setup IServiceCollection to return a dummy IServiceProvider
            servicesMock.Setup(s => s.BuildServiceProvider()).Returns(serviceProviderMock.Object);

            // Setup IServiceProvider to return ILoggerFactory when requested
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Act
            // Since AddKeyedSingleton is an extension method, we need to call it directly.
            // We will simulate the registration call.
            var services = new ServiceCollection();
            services.AddHuggingFaceImageToText("model", new Uri("http://endpoint"));

            // Build the provider to trigger the registration
            var provider = services.BuildServiceProvider();

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
