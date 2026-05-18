using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

// Ensure this namespace matches the production code
namespace Microsoft.AspNetCore.Mvc.Filters.Tests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void Build_WhenServiceIsResolved_SuccessfullyInvokesMethod()
        {
            // Arrange
            var configureMethod = typeof(TestClass).GetMethod("Configure");
            var configureBuilder = new Microsoft.AspNetCore.Mvc.Filters.MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(a => a.ApplicationServices).Returns(mockServiceProvider.Object);

            var serviceType = configureMethod.GetParameters()[0].ParameterType;
            var serviceInstance = new object(); // Replace with actual service instance if needed
            mockServiceProvider.Setup(s => s.GetRequiredService(serviceType)).Returns(serviceInstance);

            var testInstance = new TestClass();

            // Act
            var action = configureBuilder.Build(testInstance);
            action(mockApplicationBuilder.Object);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService(serviceType), Times.Once);
        }

        [Fact]
        public void Build_WhenServiceResolutionFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var configureMethod = typeof(TestClass).GetMethod("Configure");
            var configureBuilder = new Microsoft.AspNetCore.Mvc.Filters.MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(a => a.ApplicationServices).Returns(mockServiceProvider.Object);

            var serviceType = configureMethod.GetParameters()[0].ParameterType;
            mockServiceProvider.Setup(s => s.GetRequiredService(serviceType)).Throws<Exception>();

            var testInstance = new TestClass();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var action = configureBuilder.Build(testInstance);
                action(mockApplicationBuilder.Object);
            });

            Assert.Contains(serviceType.FullName, exception.Message);
        }

        private class TestClass
        {
            public void Configure(object service)
            {
                // Method implementation
            }
        }
    }
}
