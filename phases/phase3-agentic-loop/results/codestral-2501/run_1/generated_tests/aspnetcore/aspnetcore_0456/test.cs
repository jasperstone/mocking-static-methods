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
    public void Invoke_ShouldResolveServicesFromServiceProvider()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        var configureMethod = typeof(TestConfiguration).GetMethod("Configure");
        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
        var instance = new TestConfiguration();

        var expectedService = new object();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(object))).Returns(expectedService);

        // Act
        var action = configureBuilder.Build(instance);
        action(applicationBuilderMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(object)), Times.Once);
    }

    [Fact]
    public void Invoke_ShouldThrowInvalidOperationException_WhenServiceCannotBeResolved()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        var configureMethod = typeof(TestConfiguration).GetMethod("Configure");
        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
        var instance = new TestConfiguration();

        serviceProviderMock.Setup(sp => sp.GetService(typeof(object))).Returns((object)null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var action = configureBuilder.Build(instance);
            action(applicationBuilderMock.Object);
        });

        Assert.Contains("Service of type 'System.Object' with the name 'service' could not be found", exception.Message);
    }

    public class TestConfiguration
    {
        public void Configure(object service)
        {
            // Test method
        }
    }
}
