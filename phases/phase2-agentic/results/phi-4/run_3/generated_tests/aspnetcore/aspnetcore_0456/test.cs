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
        public void Invoke_ShouldResolveServicesCorrectly()
        {
            // Arrange
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var serviceType = typeof(IService);
            var serviceInstance = new Mock<IService>().Object;

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(serviceType))
                .Returns(serviceInstance);

            mockApplicationBuilder
                .Setup(app => app.ApplicationServices)
                .Returns(mockServiceProvider.Object);

            var configureMethod = typeof(TestStartup).GetMethod("Configure");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

            var instance = new TestStartup();

            // Act
            var action = configureBuilder.Build(instance);
            action(mockApplicationBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(serviceType), Times.Once);
        }

        [Fact]
        public void Invoke_ShouldThrowException_WhenServiceResolutionFails()
        {
            // Arrange
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var serviceType = typeof(IService);
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(serviceType))
                .Throws(new InvalidOperationException("Service not found"));

            mockApplicationBuilder
                .Setup(app => app.ApplicationServices)
                .Returns(mockServiceProvider.Object);

            var configureMethod = typeof(TestStartup).GetMethod("Configure");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

            var instance = new TestStartup();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var action = configureBuilder.Build(instance);
                action(mockApplicationBuilder.Object);
            });

            Assert.Contains("ServiceResolutionFail", exception.Message);
        }

        private class TestStartup
        {
            public void Configure(IApplicationBuilder app, IService service)
            {
                // Method implementation
            }
        }

        private interface IService { }
    }
}
