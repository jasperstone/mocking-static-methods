using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests;

public class FormatFilterAttributeTests
{
    [Fact]
    public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        var attribute = new FormatFilterAttribute();
        Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null!));
    }

    [Fact]
    public void CreateInstance_CallsGetRequiredService_OnValidServiceProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var mockFilter = new Mock<IFilterMetadata>().Object;
        serviceProviderMock.Setup(sp => sp.GetRequiredService<FormatFilter>()).Returns(mockFilter);
        
        var attribute = new FormatFilterAttribute();

        // Act
        var result = attribute.CreateInstance(serviceProviderMock.Object);

        // Assert
        Assert.NotNull(result);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<FormatFilter>(), Times.Once);
    }

    [Fact]
    public void IsReusable_ReturnsTrue()
    {
        var attribute = new FormatFilterAttribute();
        Assert.True(attribute.IsReusable);
    }
}
