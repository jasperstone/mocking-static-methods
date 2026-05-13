using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void LoggerFactory_ReturnsCorrectLoggerFactory()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(loggerFactoryMock.Object, loggerFactory);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoLoggerFactoryIsRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns((ILoggerFactory)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(NullLoggerFactory.Instance, loggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsCorrectServiceSelector()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceSelectorMock = new Mock<IAIServiceSelector>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns(serviceSelectorMock.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(serviceSelectorMock.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsOrderedAIServiceSelector_WhenNoServiceSelectorIsRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns((IAIServiceSelector)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
        }

        [Fact]
        public void GetRequiredService_ThrowsKernelException_WhenServiceIsNotFound()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(string))).Returns((string)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<KernelException>(() => kernel.GetRequiredService<string>());
        }
    }
}
