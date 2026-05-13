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
        public void Invoke_ShouldInvokeMethodWithCorrectParameters_WhenServiceIsAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var serviceType = typeof(object);
            var serviceInstance = new object();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(serviceType)).Returns(serviceInstance);

            var methodInfo = typeof(object).GetMethod("ToString");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act
            configureBuilder.Invoke(serviceInstance, mockApplicationBuilder.Object);

            // Assert
            methodInfo.Invoke.Verify(m => m.Invoke(serviceInstance, new object[] { mockApplicationBuilder.Object, serviceInstance }), Times.Once);
        }

        [Fact]
        public void Invoke_ShouldThrowInvalidOperationException_WhenServiceResolutionFails()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            var serviceType = typeof(object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(serviceType)).Throws<Exception>();

            var methodInfo = typeof(object).GetMethod("ToString");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => configureBuilder.Invoke(new object(), mockApplicationBuilder.Object));
            Assert.Contains("ServiceResolutionFail", exception.Message);
        }
    }
}
