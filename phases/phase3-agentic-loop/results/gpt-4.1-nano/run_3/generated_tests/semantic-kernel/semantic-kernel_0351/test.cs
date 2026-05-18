using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.SemanticKernel;

namespace KernelTests
{
    public class KernelServiceTests
    {
        [Fact]
        public void LoggerFactory_ShouldReturnService_WhenServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.IsType<LoggerFactory>(loggerFactory);
        }

        [Fact]
        public void LoggerFactory_ShouldReturnNullLoggerFactory_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.IsType<NullLoggerFactory>(loggerFactory);
        }

        [Fact]
        public void ServiceSelector_ShouldReturnService_WhenServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockSelector = new Mock<IAIServiceSelector>();
            services.AddSingleton<IAIServiceSelector>(mockSelector.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var selector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(selector);
            Assert.Equal(mockSelector.Object, selector);
        }

        [Fact]
        public void ServiceSelector_ShouldReturnDefault_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var selector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(selector);
            Assert.IsType<OrderedAIServiceSelector>(selector);
        }
    }
}
