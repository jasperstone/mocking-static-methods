using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Mvc.Tests;

public class FormatFilterAttributeTests
{
    [Fact]
    public void CreateInstance_NullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var attribute = new FormatFilterAttribute();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(serviceProvider: null!));
    }

    [Fact]
    public void CreateInstance_ServiceProviderWithoutFormatFilter_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new FormatFilterAttribute();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProvider));
    }

    [Fact]
    public void CreateInstance_ServiceProviderWithFormatFilter_ReturnsFormatFilter()
    {
        // Arrange
        var mockFormatFilter = new Mock<IFilterMetadata>();
        var attribute = new FormatFilterAttribute();
        var services = new ServiceCollection();
        services.AddSingleton<IFilterMetadata>(mockFormatFilter.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = attribute.CreateInstance(serviceProvider);

        // Assert
        Assert.Same(mockFormatFilter.Object, result);
    }

    [Fact]
    public void IsReusable_ReturnsTrue()
    {
        // Arrange
        var attribute = new FormatFilterAttribute();

        // Assert
        Assert.True(attribute.IsReusable);
    }
}
