using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Abstractions;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_ReturnsRegisteredService_WhenRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAIServiceSelector, TestAIServiceSelector>()
                .BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<TestAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void ServiceSelector_ReturnsDefaultService_WhenNotRegistered()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }

        private class TestAIServiceSelector : IAIServiceSelector
        {
            public IAIService SelectService(IServiceProvider serviceProvider)
            {
                throw new NotImplementedException();
            }
        }
    }
}
