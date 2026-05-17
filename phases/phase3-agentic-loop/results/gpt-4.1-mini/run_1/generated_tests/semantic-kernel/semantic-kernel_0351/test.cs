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
        public void Culture_DefaultsToInvariantCulture_WhenNotSet()
        {
            var kernel = new Kernel();
            Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
        }

        [Fact]
        public void Culture_SetToNull_ResetsToInvariantCulture()
        {
            var kernel = new Kernel();
            kernel.Culture = null;
            Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
        }

        [Fact]
        public void LoggerFactory_ReturnsLoggerFactoryFromServices_WhenAvailable()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(ILoggerFactory), mockLoggerFactory);

            var kernel = new Kernel(serviceProvider);

            Assert.Same(mockLoggerFactory, kernel.LoggerFactory);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenNotAvailable()
        {
            var serviceProvider = new TestServiceProvider();

            var kernel = new Kernel(serviceProvider);

            Assert.Same(NullLoggerFactory.Instance, kernel.LoggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsServiceSelectorFromServices_WhenAvailable()
        {
            var mockSelector = new Mock<IAIServiceSelector>().Object;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.AddService(typeof(IAIServiceSelector), mockSelector);

            var kernel = new Kernel(serviceProvider);

            Assert.Same(mockSelector, kernel.ServiceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefaultInstance_WhenNotAvailable()
        {
            var serviceProvider = new TestServiceProvider();

            var kernel = new Kernel(serviceProvider);

            var serviceSelector = kernel.ServiceSelector;
            Assert.NotNull(serviceSelector);
            Assert.IsAssignableFrom<IAIServiceSelector>(serviceSelector);
        }
    }
}
