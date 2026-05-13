using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters.Tests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void Invoke_ShouldResolveServicesFromServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var mockService = new Mock<ISomeService>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ISomeService))).Returns(mockService.Object);

            var methodInfo = typeof(TestConfiguration).GetMethod(nameof(TestConfiguration.Configure));
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
            var instance = new TestConfiguration();

            // Act
            var action = configureBuilder.Build(instance);
            action(mockApplicationBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ISomeService)), Times.Once);
        }

        [Fact]
        public void Invoke_ShouldThrowInvalidOperationException_WhenServiceCannotBeResolved()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ISomeService))).Returns((ISomeService)null);

            var methodInfo = typeof(TestConfiguration).GetMethod(nameof(TestConfiguration.Configure));
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
            var instance = new TestConfiguration();

            // Act & Assert
            var action = configureBuilder.Build(instance);
            Assert.Throws<InvalidOperationException>(() => action(mockApplicationBuilder.Object));
        }

        public interface ISomeService
        {
        }

        public class TestConfiguration
        {
            public void Configure(ISomeService someService)
            {
                // Do nothing
            }
        }
    }
}
