using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel;

public class WebServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBingTextSearch_WhenOptionsIsNull_GetServiceIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var options = new BingTextSearchOptions();
        serviceProviderMock.Setup(sp => sp.GetService<BingTextSearchOptions>()).Returns(options);

        // Act
        services.AddBingTextSearch("apiKey", serviceId: "testServiceId");

        // Assert
        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetRequiredService<ITextSearch>("testServiceId");
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddBingTextSearch_WhenOptionsIsProvided_GetServiceIsNotCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var options = new BingTextSearchOptions();
        var apiKey = "apiKey";

        // Act
        services.AddBingTextSearch(apiKey, options, serviceId: "testServiceId");

        // Assert
        var provider = services.BuildServiceProvider();
        var textSearch = provider.GetRequiredService<ITextSearch>("testServiceId");
        Assert.NotNull(textSearch);
    }
}
