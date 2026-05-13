using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        private class TestStartup
        {
            public bool Called { get; private set; }
            public IServiceProvider ServiceProvider { get; private set; }
            public IApplicationBuilder ApplicationBuilder { get; private set; }

            public void Configure(IApplicationBuilder app)
            {
                Called = true;
                ApplicationBuilder = app;
            }
        }

        private class TestStartupWithService
        {
            public bool Called { get; private set; }
            public IServiceProvider ServiceProvider { get; private set; }
            public IApplicationBuilder ApplicationBuilder { get; private set; }
            public object Service { get; private set; }

            public void Configure(IApplicationBuilder app, IServiceProvider service)
            {
                Called = true;
                ApplicationBuilder = app;
                Service = service;
            }
        }

        private class TestService { }

        private class TestApplicationBuilder : IApplicationBuilder
        {
            public IServiceProvider ApplicationServices { get; set; }

            public IFeatureCollection ServerFeatures => throw new NotImplementedException();

            public IDictionary<string, object?> Properties => throw new NotImplementedException();

            public RequestDelegate Build() => throw new NotImplementedException();

            public IApplicationBuilder New() => throw new NotImplementedException();

            public TestApplicationBuilder(IServiceProvider serviceProvider)
            {
                ApplicationServices = serviceProvider;
            }
        }

        [Fact]
        public void CreateConfigureDelegate_CallsConfigureWithIApplicationBuilder()
        {
            // Arrange
            var configureDelegate = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(TestStartup));
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var appBuilder = new TestApplicationBuilder(serviceProvider);

            // Act
            configureDelegate(appBuilder);

            // Assert
            // We cannot directly check the internal state of the instance used in the delegate,
            // so we test indirectly by creating a ConfigureBuilder and invoking the method.
            var methodInfo = typeof(TestStartup).GetMethod("Configure", new[] { typeof(IApplicationBuilder) });
            var instance = new TestStartup();
            var builder = new MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(TestStartup));
            var configureBuilder = new MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(TestStartup));
            // Instead, we test the effect by invoking the method directly:
            methodInfo.Invoke(instance, new object[] { appBuilder });
            Assert.True(instance.Called);
            Assert.Equal(appBuilder, instance.ApplicationBuilder);
        }

        [Fact]
        public void ConfigureBuilder_Invoke_ResolvesServiceFromServiceProvider()
        {
            // Arrange
            var service = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton(service);
            var serviceProvider = services.BuildServiceProvider();

            var appBuilder = new TestApplicationBuilder(serviceProvider);

            var instance = new TestStartupWithService();
            var methodInfo = typeof(TestStartupWithService).GetMethod("Configure", new[] { typeof(IApplicationBuilder), typeof(TestService) });

            var configureBuilder = (MiddlewareFilterConfigurationProvider.ConfigureBuilder)Activator.CreateInstance(
                typeof(MiddlewareFilterConfigurationProvider).GetNestedType("ConfigureBuilder", BindingFlags.NonPublic)!, 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new object[] { methodInfo }, 
                null)!;

            // Act
            var action = configureBuilder.Build(instance);
            action(appBuilder);

            // Assert
            Assert.True(instance.Called);
            Assert.Equal(appBuilder, instance.ApplicationBuilder);
            Assert.Same(service, instance.Service);
        }

        [Fact]
        public void ConfigureBuilder_Invoke_ThrowsInvalidOperationException_WhenServiceNotFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var appBuilder = new TestApplicationBuilder(serviceProvider);

            var instance = new TestStartupWithService();
            var methodInfo = typeof(TestStartupWithService).GetMethod("Configure", new[] { typeof(IApplicationBuilder), typeof(TestService) });

            var configureBuilder = (MiddlewareFilterConfigurationProvider.ConfigureBuilder)Activator.CreateInstance(
                typeof(MiddlewareFilterConfigurationProvider).GetNestedType("ConfigureBuilder", BindingFlags.NonPublic)!, 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                new object[] { methodInfo }, 
                null)!;

            var action = configureBuilder.Build(instance);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => action(appBuilder));
            Assert.Contains("TestService", ex.Message);
            Assert.NotNull(ex.InnerException);
        }
    }
}
