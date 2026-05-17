using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace MiddlewareFilterConfigurationProviderTests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        private class DummyService { }

        private class DummyConfigureClass
        {
            public bool WasCalled { get; private set; }
            public void Configure(DummyService service, IApplicationBuilder app)
            {
                WasCalled = true;
            }
        }

        [Fact]
        public void Invoke_Should_Call_Method_With_Correct_Parameters()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var dummyService = new DummyService();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(DummyService)))
                .Returns(dummyService);

            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder.SetupGet(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var dummyInstance = new DummyConfigureClass();

            var methodInfo = typeof(DummyConfigureClass).GetMethod("Configure");
            var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act
            var action = builder.Build(dummyInstance);
            action(mockBuilder.Object);

            // Assert
            Assert.True(dummyInstance.WasCalled);
        }

        [Fact]
        public void Invoke_Should_Throw_InvalidOperationException_When_Service_Not_Found()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(DummyService)))
                .Throws(new InvalidOperationException("Service not found"));

            var mockBuilder = new Mock<IApplicationBuilder>();
            mockBuilder.SetupGet(b => b.ApplicationServices).Returns(mockServiceProvider.Object);

            var dummyInstance = new DummyConfigureClass();

            var methodInfo = typeof(DummyConfigureClass).GetMethod("Configure");
            var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var action = builder.Build(dummyInstance);
                action(mockBuilder.Object);
            });
            Assert.Contains("DummyService", exception.Message);
        }
    }
}
