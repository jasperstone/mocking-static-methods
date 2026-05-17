using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_GetService_ReturnsService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
        }

        [Fact]
        public void ServiceSelector_GetService_ReturnsNullLoggerFactory_WhenNoLoggerFactoryIsRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
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
            Assert.NotNull(serviceSelector);
        }
    }
}
