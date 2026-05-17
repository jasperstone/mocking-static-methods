using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void ConfigureBuilder_Invoke_CallsGetRequiredServiceForNonApplicationBuilderParameters()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(string)))
                              .Returns("test");

            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder.Setup(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var configureMethod = typeof(TestConfigureClass).GetMethod("ConfigureWithServices", Array.Empty<Type>())!;
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var instance = new TestConfigureClass();

            // Act
            var action = configureBuilder.Build(instance);
            action(mockBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(string)), Times.Once());
        }

        [Fact]
        public void ConfigureBuilder_Invoke_HandlesApplicationBuilderParameterCorrectly()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder.Setup(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var configureMethod = typeof(TestConfigureClass).GetMethod("ConfigureWithBuilderOnly", Array.Empty<Type>())!;
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var instance = new TestConfigureClass();

            // Act
            var action = configureBuilder.Build(instance);
            action(mockBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Never());
        }

        [Fact]
        public void ConfigureBuilder_Invoke_ThrowsInvalidOperationExceptionWhenServiceResolutionFails()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(MissingServiceType)))
                              .Throws(new InvalidOperationException());

            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder.Setup(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var configureMethod = typeof(TestConfigureClass).GetMethod("ConfigureWithMissingService", Array.Empty<Type>())!;
            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);
            var instance = new TestConfigureClass();

            // Act & Assert
            var action = configureBuilder.Build(instance);
            var ex = Assert.Throws<InvalidOperationException>(() => action(mockBuilder.Object));
            Assert.Contains("MissingServiceType", ex.Message);
        }

        private class TestConfigureClass
        {
            public void ConfigureWithServices(IApplicationBuilder app, string service)
            {
            }

            public void ConfigureWithBuilderOnly(IApplicationBuilder app)
            {
            }

            public void ConfigureWithMissingService(IApplicationBuilder app, MissingServiceType service)
            {
            }
        }

        private class MissingServiceType
        {
        }
    }
}
