using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.Globalization;
using Xunit;

namespace SemanticKernelTests
{
    public class KernelTests
    {
        [Fact]
        public void LoggerFactory_GetService_ReturnsNullLoggerFactory_WhenNoLoggerFactoryIsRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.IsType<Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory>(loggerFactory);
        }

        [Fact]
        public void ServiceSelector_GetService_ReturnsOrderedAIServiceSelector_WhenNoAIServiceSelectorIsRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<Microsoft.SemanticKernel.Services.OrderedAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void Culture_Get_Set()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            kernel.Culture = CultureInfo.CurrentCulture;
            var culture = kernel.Culture;

            // Assert
            Assert.Equal(CultureInfo.CurrentCulture, culture);
        }

        [Fact]
        public void Data_Get_ReturnsEmptyDictionary_WhenNoDataIsSet()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var data = kernel.Data;

            // Assert
            Assert.Empty(data);
        }
    }
}
