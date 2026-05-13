using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace SemanticKernel.Abstractions.Tests
{
    public class KernelServiceProviderServiceExtensionsTests
    {
        [Fact]
        public void LoggerFactory_ReturnsLoggerFactoryFromServices_WhenAvailable()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(mockLoggerFactory, loggerFactory);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoLoggerFactoryInServices()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(NullLoggerFactory.Instance, loggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsServiceSelectorFromServices_WhenAvailable()
        {
            // Arrange
            var mockServiceSelector = new Mock<IAIServiceSelector>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns(mockServiceSelector);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(mockServiceSelector, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsOrderedAIServiceSelectorInstance_WhenNoServiceSelectorInServices()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns(null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
        }
    }
}
