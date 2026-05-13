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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var applicationBuilderMock = new Mock<IApplicationBuilder>();
            applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

            var configureMethod = typeof(TestConfiguration).GetMethod("Configure");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var instance = new TestConfiguration();

            // Act
            var action = configureBuilder.Build(instance);
            action(applicationBuilderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(TestService)), Times.Once);
        }

        [Fact]
        public void Invoke_ShouldThrowInvalidOperationException_WhenServiceResolutionFails()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestService))).Throws<InvalidOperationException>();

            var applicationBuilderMock = new Mock<IApplicationBuilder>();
            applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

            var configureMethod = typeof(TestConfiguration).GetMethod("Configure");
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var instance = new TestConfiguration();

            // Act & Assert
            var action = configureBuilder.Build(instance);
            Assert.Throws<InvalidOperationException>(() => action(applicationBuilderMock.Object));
        }

        public class TestConfiguration
        {
            public void Configure(TestService service) { }
        }

        public class TestService { }
    }
}
