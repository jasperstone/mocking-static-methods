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
        public void LoggerFactory_ShouldReturnService_WhenServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactoryResult = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactoryResult);
            Assert.IsType<LoggerFactory>(loggerFactoryResult);
        }

        [Fact]
        public void LoggerFactory_ShouldReturnNullLoggerFactory_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactoryResult = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactoryResult);
            Assert.IsType<NullLoggerFactory>(loggerFactoryResult);
        }

        [Fact]
        public void ServiceSelector_ShouldReturnService_WhenServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockService = new Mock<IAIServiceSelector>();
            services.AddSingleton<IAIServiceSelector>(mockService.Object);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var service = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IAIServiceSelector>(service);
        }

        [Fact]
        public void ServiceSelector_ShouldReturnDefault_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var service = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(service);
            Assert.Equal(OrderedAIServiceSelector.Instance, service);
        }

        [Fact]
        public void Clone_ShouldCreateDeepCopyOfKernel()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);
            var culture = new CultureInfo("en-US");
            typeof(Kernel).GetProperty("Culture").SetValue(kernel, culture);

            // Act
            var clone = kernel.Clone();

            // Assert
            Assert.NotSame(kernel, clone);
            Assert.Equal(kernel.Culture.Name, clone.Culture.Name);
        }
    }
}
