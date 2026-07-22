using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

public class KernelTests
{
    [Fact]
    public void ServiceSelector_ReturnsMockedService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockServiceSelector = new Mock<IAIServiceSelector>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns(mockServiceSelector.Object);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.Same(mockServiceSelector.Object, serviceSelector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultInstance_WhenServiceNotRegistered()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns((IAIServiceSelector)null);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.Same(OrderedAIServiceSelector.Instance, serviceSelector);
    }

    [Fact]
    public void LoggerFactory_ReturnsMockedLoggerFactory()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.Same(mockLoggerFactory.Object, loggerFactory);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenLoggerFactoryNotRegistered()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns((ILoggerFactory)null);

        var kernel = new Kernel(mockServiceProvider.Object);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, loggerFactory);
    }

    [Fact]
    public void Culture_DefaultsToInvariantCulture()
    {
        // Arrange
        var kernel = new Kernel();

        // Act
        var culture = kernel.Culture;

        // Assert
        Assert.Equal(CultureInfo.InvariantCulture, culture);
    }

    [Fact]
    public void Culture_CanBeSetToCurrentCulture()
    {
        // Arrange
        var kernel = new Kernel();
        var currentCulture = CultureInfo.CurrentCulture;

        // Act
        kernel.Culture = currentCulture;

        // Assert
        Assert.Equal(currentCulture, kernel.Culture);
    }

    [Fact]
    public void Culture_SetsToInvariantCulture_WhenSetToNull()
    {
        // Arrange
        var kernel = new Kernel();
        kernel.Culture = CultureInfo.CurrentCulture;

        // Act
        kernel.Culture = null;

        // Assert
        Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
    }
}
