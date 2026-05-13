using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void LoggerFactory_ShouldReturnNullLoggerFactory_WhenServiceReturnsNull()
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
        public void ServiceSelector_ShouldReturnDefault_WhenServiceNotRegistered()
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

        [Fact]
        public void GetService_ShouldReturnExpectedService_WhenServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.Services.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.IsType<LoggerFactory>(loggerFactory);
        }

        [Fact]
        public void Culture_ShouldDefaultToInvariantCulture()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var culture = kernel.Culture;

            // Assert
            Assert.Equal(CultureInfo.InvariantCulture, culture);
        }

        [Fact]
        public void Clone_ShouldCreateNewKernel_WithSameCultureAndServices()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(services);
            var clone = kernel.Clone();

            // Act & Assert
            Assert.NotSame(kernel, clone);
            Assert.Equal(kernel.Culture, clone.Culture);
            Assert.Same(kernel.Services, clone.Services);
        }
    }
}
