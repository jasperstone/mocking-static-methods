using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceSelector = new Mock<IAIServiceSelector>();
            mockServiceProvider
                .Setup(sp => sp.GetService<IAIServiceSelector>())
                .Returns(mockServiceSelector.Object);

            var kernel = new Kernel(mockServiceProvider.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(mockServiceSelector.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefaultIfNoServiceFound()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService<IAIServiceSelector>())
                .Returns((IAIServiceSelector)null);

            var kernel = new Kernel(mockServiceProvider.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
        }
    }
}
