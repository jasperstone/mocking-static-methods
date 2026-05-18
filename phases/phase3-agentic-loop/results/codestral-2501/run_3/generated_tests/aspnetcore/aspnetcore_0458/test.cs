using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class FormatFilterAttributeTests
{
    [Fact]
    public void CreateInstance_ShouldReturnFormatFilter()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockFormatFilter = new Mock<FormatFilter>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService(typeof(FormatFilter)))
            .Returns(mockFormatFilter.Object);

        var attribute = new FormatFilterAttribute();

        // Act
        var result = attribute.CreateInstance(mockServiceProvider.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FormatFilter>(result);
    }

    [Fact]
    public void CreateInstance_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange
        var attribute = new FormatFilterAttribute();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null));
    }
}
