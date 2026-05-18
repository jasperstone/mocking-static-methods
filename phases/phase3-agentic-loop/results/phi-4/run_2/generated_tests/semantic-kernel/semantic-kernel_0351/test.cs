using System;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Microsoft.SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_ReturnsCorrectServiceSelector()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var expectedServiceSelector = new Mock<IAIServiceSelector>();
            serviceProviderMock.Setup(sp => sp.GetService<IAIServiceSelector>()).Returns(expectedServiceSelector.Object);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.Same(expectedServiceSelector.Object, serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefaultBehaviorWhenNoServiceFound()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IAIServiceSelector>()).Returns((IAIServiceSelector)null);

            var kernel = new Kernel(serviceProviderMock.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            // Verify that the default behavior is invoked
            Assert.NotNull(serviceSelector);
            // Optionally, verify specific behavior if possible
        }
    }
}
