using Xunit;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;

public class ControllerSaveTempDataPropertyFilterFactoryTests
{
    [Fact]
    public void CreateInstance_ShouldReturnControllerSaveTempDataPropertyFilterWithPropertiesSet()
    {
        // Arrange
        var properties = new List<LifecycleProperty>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
        var filterMock = new Mock<ControllerSaveTempDataPropertyFilter>(tempDataFactoryMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
            .Returns(filterMock.Object);

        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act
        var result = factory.CreateInstance(serviceProviderMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(filterMock.Object, result);
        Assert.Same(properties, filterMock.Object.Properties);
    }
}
