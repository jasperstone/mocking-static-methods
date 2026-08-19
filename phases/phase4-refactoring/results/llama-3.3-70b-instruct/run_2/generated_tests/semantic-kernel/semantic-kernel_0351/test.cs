using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

public class KernelTests
{
    [Fact]
    public void ServiceSelector_GetService_ReturnsService()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IAIServiceSelector, OrderedAIServiceSelector>()
            .BuildServiceProvider();

        var kernel = new Kernel(serviceProvider);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(serviceSelector);
    }
}
