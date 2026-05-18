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
    public void CreateConfigureDelegate_ValidConfigurationType_ShouldReturnAction()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockApplicationBuilder = new Mock<IApplicationBuilder>();
        mockApplicationBuilder.SetupGet(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

        var configurationType = typeof(TestConfiguration);

        // Act
        var configureDelegate = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(configurationType);
        configureDelegate(mockApplicationBuilder.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(TestService)), Times.Once);
    }

    private class TestConfiguration
    {
        public void Configure(IApplicationBuilder app, TestService service)
        {
            // Test configuration method
        }
    }

    private class TestService
    {
        // Test service
    }
}
