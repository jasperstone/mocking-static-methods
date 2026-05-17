using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using System.Reflection;

public class KernelTests
{
    [Fact]
    public void ServiceSelector_ReturnsProvidedService_WhenAvailable()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockServiceSelector = new Mock<IAIServiceSelector>();
        mockServiceProvider.Setup(sp => sp.GetService<IAIServiceSelector>()).Returns(mockServiceSelector.Object);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Same(mockServiceSelector.Object, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefault_WhenNoServiceAvailable()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService<IAIServiceSelector>()).Returns((IAIServiceSelector)null);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Same(GetOrderedAIServiceSelectorInstance(), result);
    }

    // Use reflection to access the internal instance
    public static OrderedAIServiceSelector GetOrderedAIServiceSelectorInstance()
    {
        var type = typeof(OrderedAIServiceSelector);
        var field = type.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        return (OrderedAIServiceSelector)field.GetValue(null);
    }
}
