using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace SemanticKernel.Abstractions.Tests
{
    public class KernelTests
    {
        private class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object?> _services = new();

            public void AddService(Type serviceType, object? implementation)
            {
                _services[serviceType] = implementation;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        [Fact]
        public void LoggerFactory_ReturnsLoggerFactoryFromServices_WhenAvailable()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(ILoggerFactory), mockLoggerFactory);
            serviceProvider.AddService(typeof(KernelPluginCollection), null);
            serviceProvider.AddService(typeof(IEnumerable<KernelPlugin>), Array.Empty<KernelPlugin>());

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(mockLoggerFactory, loggerFactory);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenNotAvailable()
        {
            // Arrange
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(ILoggerFactory), null);
            serviceProvider.AddService(typeof(KernelPluginCollection), null);
            serviceProvider.AddService(typeof(IEnumerable<KernelPlugin>), Array.Empty<KernelPlugin>());

            var kernel = new Kernel(serviceProvider);

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.Same(NullLoggerFactory.Instance, loggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsServiceSelectorFromServices_WhenAvailable()
        {
            // Arrange
            var mockServiceSelector = new Mock<IAIServiceSelector>().Object;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(IAIServiceSelector), mockServiceSelector);
            serviceProvider.AddService(typeof(KernelPluginCollection), null);
            serviceProvider.AddService(typeof(IEnumerable<KernelPlugin>), Array.Empty<KernelPlugin>());

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(mockServiceSelector, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsOrderedAIServiceSelectorInstance_WhenNotAvailable()
        {
            // Arrange
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(IAIServiceSelector), null);
            serviceProvider.AddService(typeof(KernelPluginCollection), null);
            serviceProvider.AddService(typeof(IEnumerable<KernelPlugin>), Array.Empty<KernelPlugin>());

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.IsAssignableFrom<IAIServiceSelector>(serviceSelector);
            Assert.Equal("OrderedAIServiceSelector", serviceSelector.GetType().Name);
        }
    }
}
