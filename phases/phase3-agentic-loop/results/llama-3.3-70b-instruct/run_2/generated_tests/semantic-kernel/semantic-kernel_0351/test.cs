using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;

namespace SemanticKernelTests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_GetService_ReturnsService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceSelectorMock = new Mock<IAIServiceSelector>();
            serviceProviderMock.Setup(p => p.GetService(typeof(IAIServiceSelector))).Returns(serviceSelectorMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(KernelPluginCollection))).Returns(new KernelPluginCollection());
            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(serviceSelectorMock.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_GetService_ReturnsDefaultService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService(typeof(IAIServiceSelector))).Returns(null);
            serviceProviderMock.Setup(p => p.GetService(typeof(KernelPluginCollection))).Returns(new KernelPluginCollection());
            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsAssignableFrom<IAIServiceSelector>(serviceSelector);
        }
    }
}
