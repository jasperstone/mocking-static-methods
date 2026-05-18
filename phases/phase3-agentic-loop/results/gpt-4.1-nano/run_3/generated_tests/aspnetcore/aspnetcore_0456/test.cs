using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MiddlewareFilterTests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void Invoke_CallsGetRequiredServiceAndMethod()
        {
            // Arrange
            var serviceType = typeof(string);
            var parameterName = "testParam";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var expectedServiceInstance = "Hello, Service!";
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(serviceType))
                .Returns(expectedServiceInstance);

            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder
                .SetupGet(b => b.ApplicationServices)
                .Returns(mockServiceProvider.Object);

            var methodInfo = typeof(TestClass).GetMethod(nameof(TestClass.TestMethod));
            var instance = new TestClass();

            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act
            var action = configureBuilder.Build(instance);
            action.Invoke(mockBuilder.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(serviceType), Times.Once);
            Assert.True(instance.WasInvoked);
        }

        private class TestClass
        {
            public bool WasInvoked { get; private set; } = false;

            public void TestMethod(string message)
            {
                WasInvoked = true;
                Assert.Equal("Hello, Service!", message);
            }
        }
    }
}
