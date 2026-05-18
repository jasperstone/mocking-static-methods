using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.SemanticKernel.Services;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void GetServiceSelector_ShouldReturnServiceSelector()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceSelector = new Mock<IAIServiceSelector>();

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IAIServiceSelector)))
                .Returns(mockServiceSelector.Object);

            var kernel = new Kernel(mockServiceProvider.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.Same(mockServiceSelector.Object, serviceSelector);
        }

        [Fact]
        public void GetServiceSelector_ShouldReturnOrderedAIServiceSelector_WhenServiceNotFound()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IAIServiceSelector)))
                .Returns((IAIServiceSelector)null);

            var kernel = new Kernel(mockServiceProvider.Object);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.NotNull(serviceSelector);
            Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
        }
    }
}
