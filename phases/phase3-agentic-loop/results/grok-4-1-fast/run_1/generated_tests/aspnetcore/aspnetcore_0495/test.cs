using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(NullLogger.Instance);
        
        var filter = new RequestFormLimitsFilter(loggerFactoryMock.Object);
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
            .Returns(filter);

        // Act
        var result = attribute.CreateInstance(serviceProviderMock.Object);

        // Assert
        Assert.Same(filter, result);
        Assert.Equal(100, filter.FormOptions.ValueCountLimit);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
    }

    [Fact]
    public void CreateInstance_WhenServiceNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var attribute = new RequestFormLimitsAttribute();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
            .Throws(new InvalidOperationException("No service for type 'RequestFormLimitsFilter' has been registered."));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProviderMock.Object));
        Assert.Contains("RequestFormLimitsFilter", exception.Message);
    }
}
