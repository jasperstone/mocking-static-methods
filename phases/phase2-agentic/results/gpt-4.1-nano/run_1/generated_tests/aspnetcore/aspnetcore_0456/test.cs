using System;
using System.Reflection;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MiddlewareFilterConfigurationProviderTests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        private class DummyService { }

        private class TestConfigureClass
        {
            public bool WasCalled { get; private set; } = false;
            public void Configure(IApplicationBuilder app, DummyService service)
            {
                WasCalled = true;
            }
        }

        [Fact]
        public void Invoke_Should_Call_Method_With_Correct_Services()
        {
            // Arrange
            var configureMethod = typeof(TestConfigureClass).GetMethod("Configure");
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var instance = new TestConfigureClass();

            var provider = new ServiceCollection()
                .AddTransient<DummyService>()
                .BuildServiceProvider();

            builder.ApplicationServices = provider;

            var configBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

            // Act
            var action = configBuilder.Build(instance);
            action(builder);

            // Assert
            Assert.True(instance.WasCalled);
        }

        [Fact]
        public void Invoke_Should_Throw_When_Service_Not_Registered()
        {
            // Arrange
            var configureMethod = typeof(TestConfigureClass).GetMethod("Configure");
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var instance = new TestConfigureClass();

            // No services registered
            builder.ApplicationServices = new ServiceCollection().BuildServiceProvider();

            var configBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => {
                var action = configBuilder.Build(instance);
                action(builder);
            });
            Assert.Contains("Could not resolve service for type", exception.Message);
        }
    }
}
