using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace SemanticKernelTests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_GetService_ReturnsServiceSelectorInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceSelectorMock = new Mock<IAIServiceSelector>();
            serviceProviderMock.Setup(p => p.GetService<IAIServiceSelector>()).Returns(serviceSelectorMock.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(serviceSelectorMock.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_GetService_ReturnsOrderedAIServiceSelectorInstance_WhenNoServiceSelectorIsRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetService<IAIServiceSelector>()).Returns(null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
        }
    }
}
