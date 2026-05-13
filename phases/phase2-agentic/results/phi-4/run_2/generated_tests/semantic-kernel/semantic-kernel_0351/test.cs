using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Xunit;

public class KernelTests
{
    [Fact]
    public void ServiceSelector_ReturnsExpectedService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var expectedServiceSelector = new Mock<IAIServiceSelector>().Object;

        mockServiceProvider
            .Setup(sp => sp.GetService<IAIServiceSelector>())
            .Returns(expectedServiceSelector);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.Same(expectedServiceSelector, serviceSelector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultService_WhenNoServiceProvided()
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
