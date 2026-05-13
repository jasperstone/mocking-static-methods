using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters.Tests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void Invoke_ShouldCallGetRequiredService_WhenParameterIsNotIApplicationBuilder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var mockService = new object();
            var parameterType = typeof(object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(parameterType))
                .Returns(mockService);

            mockApplicationBuilder
                .SetupProperty(app => app.ApplicationServices, mockServiceProvider.Object);

            var configureMethod = typeof(MiddlewareFilterConfigurationProviderTests).GetMethod(nameof(ConfigureMethod), BindingFlags.NonPublic | BindingFlags.Instance);
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

            // Act
            var action = configureBuilder.Build(null);
            action(mockApplicationBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(parameterType), Times.Once);
        }

        private void ConfigureMethod(IApplicationBuilder app, object service)
        {
            // This method is just a placeholder to simulate a real Configure method
        }
    }
}
