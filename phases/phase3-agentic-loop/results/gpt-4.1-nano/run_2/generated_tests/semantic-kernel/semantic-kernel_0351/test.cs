using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;

namespace SemanticKernel.Tests
{
    public class KernelServiceProviderExtensionsTests
    {
        [Fact]
        public void GetService_ReturnsExpectedService()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton<IAIServiceSelector, DummyAIServiceSelector>();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactoryResult = kernel.Services.GetService<ILoggerFactory>();
            var aiServiceSelector = kernel.Services.GetService<IAIServiceSelector>();

            // Assert
            Assert.NotNull(loggerFactoryResult);
            Assert.IsType<LoggerFactory>(loggerFactoryResult);
            Assert.NotNull(aiServiceSelector);
            Assert.IsType<DummyAIServiceSelector>(aiServiceSelector);
        }

        [Fact]
        public void LoggerFactoryProperty_ReturnsServiceOrNullLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactoryProperty = kernel.LoggerFactory;

            // Assert
            Assert.NotNull(loggerFactoryProperty);
            Assert.IsType<LoggerFactory>(loggerFactoryProperty);
        }

        [Fact]
        public void ServiceSelectorProperty_ReturnsServiceOrDefault()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IAIServiceSelector, DummyAIServiceSelector>();
            var serviceProvider = services.BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.IsType<DummyAIServiceSelector>(serviceSelector);
        }

        private class DummyAIServiceSelector : IAIServiceSelector
        {
            // Implement interface members if needed
        }
    }
}
