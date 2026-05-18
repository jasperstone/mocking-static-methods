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
    public void Culture_DefaultsToInvariantCulture()
    {
        // Arrange
        var kernel = new Kernel();

        // Act & Assert
        Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
    }

    [Fact]
    public void Culture_CanBeSet()
    {
        // Arrange
        var kernel = new Kernel();
        var culture = new CultureInfo("fr-FR");

        // Act
        kernel.Culture = culture;

        // Assert
        Assert.Equal(culture, kernel.Culture);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoLoggerFactoryInServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns((ILoggerFactory)null);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.IsType<NullLoggerFactory>(loggerFactory);
    }

    [Fact]
    public void LoggerFactory_ReturnsLoggerFactory_WhenLoggerFactoryInServices()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactoryMock.Object, loggerFactory);
    }

    [Fact]
    public void ServiceSelector_ReturnsOrderedAIServiceSelector_WhenNoAIServiceSelectorInServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns((IAIServiceSelector)null);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(serviceSelector);
    }

    [Fact]
    public void ServiceSelector_ReturnsAIServiceSelector_WhenAIServiceSelectorInServices()
    {
        // Arrange
        var aiServiceSelectorMock = new Mock<IAIServiceSelector>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAIServiceSelector))).Returns(aiServiceSelectorMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        var serviceSelector = kernel.ServiceSelector;

        // Assert
        Assert.Same(aiServiceSelectorMock.Object, serviceSelector);
    }

    [Fact]
    public void Data_ReturnsNewDictionary_WhenDataIsNull()
    {
        // Arrange
        var kernel = new Kernel();

        // Act
        var data = kernel.Data;

        // Assert
        Assert.NotNull(data);
        Assert.Empty(data);
    }

    [Fact]
    public void GetRequiredService_ThrowsKernelException_WhenServiceNotFound()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(string))).Returns((string)null);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act & Assert
        Assert.Throws<KernelException>(() => kernel.GetRequiredService<string>());
    }

    [Fact]
    public void GetRequiredService_ReturnsService_WhenServiceFound()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(string))).Returns("TestService");
        serviceProviderMock.Setup(sp => sp.GetService(typeof(KernelPluginCollection))).Returns((KernelPluginCollection)null);
        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        var service = kernel.GetRequiredService<string>();

        // Assert
        Assert.Equal("TestService", service);
    }
}
