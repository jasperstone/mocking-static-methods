using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace SemanticKernelTests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_ReturnsServiceFromServiceProvider_IfAvailable()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceSelectorMock = new Mock<IAIServiceSelector>();
            serviceProviderMock.Setup(p => p.GetService(typeof(IAIServiceSelector))).Returns(serviceSelectorMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IEnumerable<KernelPlugin>))).Returns(new List<KernelPlugin>());
            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(serviceSelectorMock.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefaultServiceSelector_IfNotAvailableInServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService(typeof(IAIServiceSelector))).Returns(null);
            serviceProviderMock.Setup(p => p.GetService(typeof(IEnumerable<KernelPlugin>))).Returns(new List<KernelPlugin>());
            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
        }
    }
}
