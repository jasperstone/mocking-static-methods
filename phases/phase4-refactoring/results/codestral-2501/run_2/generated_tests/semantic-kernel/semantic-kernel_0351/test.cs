using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.Functions;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void Kernel_InitializesWithDefaultServices()
        {
            // Arrange
            var kernel = new Kernel();

            // Act & Assert
            Assert.NotNull(kernel.Services);
            Assert.IsType<EmptyServiceProvider>(kernel.Services);
        }

        [Fact]
        public void Kernel_InitializesWithProvidedServices()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act & Assert
            Assert.Same(serviceProvider, kernel.Services);
        }

        [Fact]
        public void Kernel_InitializesWithProvidedPlugins()
        {
            // Arrange
            var plugins = new KernelPluginCollection();
            var kernel = new Kernel(plugins: plugins);

            // Act & Assert
            Assert.Same(plugins, kernel.Plugins);
        }

        [Fact]
        public void Kernel_InitializesWithDefaultPlugins()
        {
            // Arrange
            var kernel = new Kernel();

            // Act & Assert
            Assert.NotNull(kernel.Plugins);
            Assert.IsType<KernelPluginCollection>(kernel.Plugins);
        }

        [Fact]
        public void Kernel_LoggerFactory_ReturnsNullLoggerFactoryWhenNoLoggerFactoryIsProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.IsType<NullLoggerFactory>(loggerFactory);
        }

        [Fact]
        public void Kernel_LoggerFactory_ReturnsProvidedLoggerFactory()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var actualLoggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(loggerFactory, actualLoggerFactory);
        }

        [Fact]
        public void Kernel_ServiceSelector_ReturnsOrderedAIServiceSelectorWhenNoServiceSelectorIsProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void Kernel_ServiceSelector_ReturnsProvidedServiceSelector()
        {
            // Arrange
            var serviceSelector = new MockAIServiceSelector();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAIServiceSelector>(serviceSelector)
                .BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var actualServiceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(serviceSelector, actualServiceSelector);
        }

        [Fact]
        public void Kernel_Data_ReturnsNewDictionary()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var data = kernel.Data;

            // Assert
            Assert.NotNull(data);
            Assert.IsType<Dictionary<string, object?>>(data);
        }

        [Fact]
        public void Kernel_Culture_ReturnsInvariantCultureByDefault()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var culture = kernel.Culture;

            // Assert
            Assert.Equal(CultureInfo.InvariantCulture, culture);
        }

        [Fact]
        public void Kernel_Culture_CanBeSet()
        {
            // Arrange
            var kernel = new Kernel();
            var newCulture = new CultureInfo("fr-FR");

            // Act
            kernel.Culture = newCulture;

            // Assert
            Assert.Equal(newCulture, kernel.Culture);
        }

        [Fact]
        public void Kernel_Clone_CreatesNewInstanceWithSameServices()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var kernel = new Kernel(serviceProvider);

            // Act
            var clonedKernel = kernel.Clone();

            // Assert
            Assert.NotSame(kernel, clonedKernel);
            Assert.Same(kernel.Services, clonedKernel.Services);
        }

        [Fact]
        public void Kernel_Clone_CreatesNewInstanceWithSamePlugins()
        {
            // Arrange
            var plugins = new KernelPluginCollection();
            var kernel = new Kernel(plugins: plugins);

            // Act
            var clonedKernel = kernel.Clone();

            // Assert
            Assert.NotSame(kernel.Plugins, clonedKernel.Plugins);
            Assert.Equal(kernel.Plugins, clonedKernel.Plugins);
        }

        [Fact]
        public void Kernel_Clone_CreatesNewInstanceWithSameData()
        {
            // Arrange
            var kernel = new Kernel();
            kernel.Data["key"] = "value";

            // Act
            var clonedKernel = kernel.Clone();

            // Assert
            Assert.NotSame(kernel.Data, clonedKernel.Data);
            Assert.Equal(kernel.Data, clonedKernel.Data);
        }

        [Fact]
        public void Kernel_Clone_CreatesNewInstanceWithSameCulture()
        {
            // Arrange
            var kernel = new Kernel();
            kernel.Culture = new CultureInfo("fr-FR");

            // Act
            var clonedKernel = kernel.Clone();

            // Assert
            Assert.Same(kernel.Culture, clonedKernel.Culture);
        }

        private class MockAIServiceSelector : IAIServiceSelector
        {
            public IReadOnlyList<AIService> GetServices(string serviceType) => throw new NotImplementedException();

            public bool TrySelectAIService<T>(Kernel kernel, KernelFunction function, KernelArguments arguments, out T? service, out PromptExecutionSettings? settings) where T : class
            {
                service = null;
                settings = null;
                return false;
            }
        }
    }
}
