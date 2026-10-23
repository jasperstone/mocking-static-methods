using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;

namespace SemanticKernel.Tests
{
    public class KernelServiceTests
    {
        [Fact]
        public void LoggerFactory_Should_Return_LoggerFactory_From_ServiceProvider_When_Available()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.Equal(mockLoggerFactory.Object, loggerFactory);
        }

        [Fact]
        public void LoggerFactory_Should_Return_NullLoggerFactory_When_ServiceProvider_Does_Not_Have_ILoggerFactory()
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
        public void ServiceSelector_Should_Return_Service_From_ServiceProvider_When_Available()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceSelector = new Mock<IAIServiceSelector>();
            services.AddSingleton<IAIServiceSelector>(mockServiceSelector.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.Equal(mockServiceSelector.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_Should_Return_Default_When_ServiceProvider_Does_Not_Have_Service()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }
    }
}
