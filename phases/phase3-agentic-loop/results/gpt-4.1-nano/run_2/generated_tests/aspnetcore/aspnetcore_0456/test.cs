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

        private class TestConfigureClass
        {
            public bool WasCalled { get; private set; }
            public void Configure(IApplicationBuilder app, DummyService service)
            {
                WasCalled = true;
            }
        }

        [Fact]
        public void Invoke_Should_Call_Method_With_Correct_Services()
        {
            // Arrange
            var methodInfo = typeof(TestConfigureClass).GetMethod("Configure");
            var builderMock = new Mock<IApplicationBuilder>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var testInstance = new TestConfigureClass();

            // Setup ApplicationServices to return a DummyService
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(DummyService)))
                .Returns(new DummyService());

            builderMock.SetupGet(b => b.ApplicationServices).Returns(serviceProviderMock.Object);

            var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act
            var action = configureBuilder.Build(testInstance);
            action(builderMock.Object);

            // Assert
            Assert.True(testInstance.WasCalled);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(DummyService)), Times.Once);
        }
    }
}
