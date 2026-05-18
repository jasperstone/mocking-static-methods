using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc;

public class RequestFormLimitsAttributeTests
{
    [Fact]
    public void CreateInstance_WhenCalled_ReturnsFilterWithFormOptions()
    {
        // Arrange
        var attribute = new RequestFormLimitsAttribute
        {
            ValueCountLimit = 100
        };

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var filter = new RequestFormLimitsFilter(loggerFactoryMock.Object);
        var services = new ServiceCollection();
        services.AddSingleton(filter);
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var result = attribute.CreateInstance(serviceProvider);

        // Assert
        Assert.Same(filter, result);
        Assert.Same(attribute.FormOptions, ((RequestFormLimitsFilter)result).FormOptions);
        Assert.Equal(100, ((RequestFormLimitsFilter)result).FormOptions.ValueCountLimit);
    }

    [Fact]
    public void CreateInstance_WhenServiceNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new RequestFormLimitsAttribute();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
            .Throws(new InvalidOperationException("No service for type 'RequestFormLimitsFilter'."));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProviderMock.Object));
        Assert.Equal("No service for type 'RequestFormLimitsFilter'.", ex.Message);
    }
}
