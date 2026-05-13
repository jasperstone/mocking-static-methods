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
        public void GetRequiredService_WithServiceKey_ShouldReturnService()
        {
            // Arrange
            var serviceKey = new object();
            var expectedService = new Mock<IAIServiceSelector>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
                .Returns(expectedService);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.GetRequiredService<IAIServiceSelector>(serviceKey);

            // Assert
            Assert.Equal(expectedService, result);
        }

        [Fact]
        public void GetRequiredService_WithoutServiceKey_ShouldReturnService()
        {
            // Arrange
            var expectedService = new Mock<IAIServiceSelector>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
                .Returns(expectedService);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.GetRequiredService<IAIServiceSelector>();

            // Assert
            Assert.Equal(expectedService, result);
        }

        [Fact]
        public void GetRequiredService_ServiceNotFound_ShouldThrowKernelException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
                .Returns((IAIServiceSelector)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<KernelException>(() => kernel.GetRequiredService<IAIServiceSelector>());
        }

        [Fact]
        public void LoggerFactory_ShouldReturnLoggerFactoryFromServices()
        {
            // Arrange
            var expectedLoggerFactory = new Mock<ILoggerFactory>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(expectedLoggerFactory);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.LoggerFactory;

            // Assert
            Assert.Equal(expectedLoggerFactory, result);
        }

        [Fact]
        public void LoggerFactory_ServiceNotFound_ShouldReturnNullLoggerFactory()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns((ILoggerFactory)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.LoggerFactory;

            // Assert
            Assert.Equal(NullLoggerFactory.Instance, result);
        }

        [Fact]
        public void ServiceSelector_ShouldReturnServiceSelectorFromServices()
        {
            // Arrange
            var expectedServiceSelector = new Mock<IAIServiceSelector>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
                .Returns(expectedServiceSelector);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.ServiceSelector;

            // Assert
            Assert.Equal(expectedServiceSelector, result);
        }

        [Fact]
        public void ServiceSelector_ServiceNotFound_ShouldReturnOrderedAIServiceSelector()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
                .Returns((IAIServiceSelector)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var result = kernel.ServiceSelector;

            // Assert
            Assert.Equal(OrderedAIServiceSelector.Instance, result);
        }

        [Fact]
        public void Culture_ShouldReturnInvariantCultureByDefault()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var result = kernel.Culture;

            // Assert
            Assert.Equal(CultureInfo.InvariantCulture, result);
        }

        [Fact]
        public void Culture_ShouldSetAndReturnCulture()
        {
            // Arrange
            var kernel = new Kernel();
            var expectedCulture = CultureInfo.GetCultureInfo("fr-FR");

            // Act
            kernel.Culture = expectedCulture;
            var result = kernel.Culture;

            // Assert
            Assert.Equal(expectedCulture, result);
        }

        [Fact]
        public void Culture_ShouldSetToInvariantCultureIfNull()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            kernel.Culture = null;
            var result = kernel.Culture;

            // Assert
            Assert.Equal(CultureInfo.InvariantCulture, result);
        }
    }
}
