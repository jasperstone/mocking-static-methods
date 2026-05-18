using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class MiddlewareFilterConfigurationProviderTests
{
    [Fact]
    public void CreateConfigureDelegate_ValidConfigurationType_ReturnsAction()
    {
        // Arrange
        var configurationType = typeof(ValidConfiguration);

        // Act
        var result = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(configurationType);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CreateConfigureDelegate_InvalidConfigurationType_ThrowsException()
    {
        // Arrange
        var configurationType = typeof(InvalidConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(configurationType));
    }

    [Fact]
    public void Invoke_ValidServiceProvider_ResolvesServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ValidService))).Returns(new ValidService());

        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(typeof(ValidConfiguration).GetMethod("Configure"));
        var instance = Activator.CreateInstance(typeof(ValidConfiguration));

        // Act
        var action = configureBuilder.Build(instance);
        action(applicationBuilderMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ValidService)), Times.Once);
    }

    [Fact]
    public void Invoke_InvalidServiceProvider_ThrowsException()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ValidService))).Returns((ValidService)null);

        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(typeof(ValidConfiguration).GetMethod("Configure"));
        var instance = Activator.CreateInstance(typeof(ValidConfiguration));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => configureBuilder.Build(instance)(applicationBuilderMock.Object));
    }

    public class ValidConfiguration
    {
        public void Configure(IApplicationBuilder app, ValidService service)
        {
            // Do nothing
        }
    }

    public class InvalidConfiguration
    {
        // No parameterless constructor
    }

    public class ValidService
    {
        // Dummy service
    }
}
