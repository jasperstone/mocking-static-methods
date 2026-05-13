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
        [Fact]
        public void Culture_SetNull_ResetsToInvariantCulture()
        {
            var kernel = new Kernel();
            kernel.Culture = null;
            Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
        }

        [Fact]
        public void LoggerFactory_ReturnsServiceFromServices()
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory);

            var kernel = new Kernel(servicesMock.Object);

            Assert.Same(mockLoggerFactory, kernel.LoggerFactory);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactoryWhenServiceNotFound()
        {
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(null);

            var kernel = new Kernel(servicesMock.Object);

            Assert.Same(NullLoggerFactory.Instance, kernel.LoggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsServiceFromServices()
        {
            var mockServiceSelector = new Mock<IAIServiceSelector>().Object;
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IAIServiceSelector))).Returns(mockServiceSelector);

            var kernel = new Kernel(servicesMock.Object);

            Assert.Same(mockServiceSelector, kernel.ServiceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsOrderedAIServiceSelectorWhenServiceNotFound()
        {
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IAIServiceSelector))).Returns(null);

            var kernel = new Kernel(servicesMock.Object);

            Assert.Same(OrderedAIServiceSelector.Instance, kernel.ServiceSelector);
        }
    }
}
