using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel; // Add this using directive

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WhenOptionsIsNull_UsesServiceProviderToGetOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var options = new BingTextSearchOptions();
        serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

        // Act
        services.AddBingTextSearch("apiKey", null, "serviceId");

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService<BingTextSearchOptions>(), Times.Once);
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsIsProvided_DoesNotUseServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var options = new BingTextSearchOptions();

        // Act
        services.AddBingTextSearch("apiKey", options, "serviceId");

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService<BingTextSearchOptions>(), Times.Never);
    }
}
