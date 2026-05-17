using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel;

namespace KernelTests
{
    public class KernelServiceTests
    {
        [Fact]
        public void LoggerFactory_ReturnsService_WhenAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act
            var result = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LoggerFactory>(result);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenServiceNotAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act
            var result = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NullLoggerFactory>(result);
        }

        [Fact]
        public void ServiceSelector_ReturnsService_WhenAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockSelector = new Moq.Mock<IAIServiceSelector>();
            services.AddSingleton<IAIServiceSelector>(mockSelector.Object);
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act
            var result = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockSelector.Object, result);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefault_WhenNotAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act
            var result = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderedAIServiceSelector>(result);
        }

        [Fact]
        public void GetRequiredService_ReturnsService_WhenAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var testService = new object();
            services.AddSingleton(testService);
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act
            var result = kernel.GetRequiredService<object>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testService, result);
        }

        [Fact]
        public void GetRequiredService_Throws_WhenServiceNotFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();
            var kernel = new Kernel(provider);

            // Act & Assert
            Assert.Throws<KernelException>(() => kernel.GetRequiredService<IDisposable>());
        }
    }
}
