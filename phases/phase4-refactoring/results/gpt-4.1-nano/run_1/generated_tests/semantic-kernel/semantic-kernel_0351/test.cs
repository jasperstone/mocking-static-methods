using Xunit;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Moq;

namespace SemanticKernel.Tests
{
    public class KernelServiceTests
    {
        [Fact]
        public void LoggerFactory_Should_Return_Service_When_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.Equal(mockLoggerFactory.Object, loggerFactory);
        }

        [Fact]
        public void LoggerFactory_Should_Return_NullLoggerFactory_When_Service_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.IsType<NullLoggerFactory>(loggerFactory);
        }

        [Fact]
        public void ServiceSelector_Should_Return_Service_When_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceSelector = new Mock<IAIServiceSelector>();
            services.AddSingleton<IAIServiceSelector>(mockServiceSelector.Object);
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.Equal(mockServiceSelector.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_Should_Return_Default_When_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.IsType<Microsoft.SemanticKernel.Services.OrderedAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void GetRequiredService_Should_Return_Service_When_Found()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockService = new object();
            services.AddSingleton<object>(mockService);
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var result = kernel.GetRequiredService<object>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockService, result);
        }

        [Fact]
        public void GetRequiredService_Should_Throw_When_Service_Not_Found()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => kernel.GetRequiredService<IDisposable>());
        }
    }
}
