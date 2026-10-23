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
        public void LoggerFactory_Should_Return_NullLoggerFactory_When_Not_Registered()
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
        public void ServiceSelector_Should_Return_OrderedAIServiceSelector_When_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void GetRequiredService_Should_Return_Service_When_Found()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockService = new object();
            services.AddSingleton(mockService);
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var service = kernel.GetRequiredService<object>();

            // Assert
            Assert.NotNull(service);
            Assert.Equal(mockService, service);
        }

        [Fact]
        public void GetRequiredService_Should_Use_KeyedService_When_ServiceKey_Is_Not_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockService = new object();
            var mockKeyedProvider = new Mock<IKeyedServiceProvider>();
            mockKeyedProvider.Setup(p => p.GetKeyedService<object>(It.IsAny<object>())).Returns(mockService);
            services.AddSingleton<IKeyedServiceProvider>(mockKeyedProvider.Object);
            var provider = services.BuildServiceProvider();

            var kernel = new Kernel(provider);

            // Act
            var result = kernel.GetRequiredService<object>(serviceKey: "key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockService, result);
        }
    }
}
