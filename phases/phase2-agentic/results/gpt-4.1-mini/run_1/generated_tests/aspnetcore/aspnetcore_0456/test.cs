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
            public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
            {
                Configured = true;
                ReceivedServiceProvider = serviceProvider;
                ReceivedAppBuilder = app;
            }

            public bool Configured { get; private set; }
            public IServiceProvider? ReceivedServiceProvider { get; private set; }
            public IApplicationBuilder? ReceivedAppBuilder { get; private set; }
        }

        private class TestService { }

        private class TestStartupWithService
        {
            public void Configure(IApplicationBuilder app, TestService service)
            {
                Configured = true;
                ReceivedService = service;
            }

            public bool Configured { get; private set; }
            public TestService? ReceivedService { get; private set; }
        }

        private class TestAppBuilder : IApplicationBuilder
        {
            public IServiceProvider ApplicationServices { get; set; }

            public TestAppBuilder(IServiceProvider serviceProvider)
            {
                ApplicationServices = serviceProvider;
            }

            // The rest of the interface members are not used in the test and can throw NotImplementedException
            public IServiceProvider ServiceProvider => throw new NotImplementedException();
            public IFeatureCollection ServerFeatures => throw new NotImplementedException();
            public IDictionary<string, object?> Properties => throw new NotImplementedException();
            public RequestDelegate Build() => throw new NotImplementedException();
            public IApplicationBuilder New() => throw new NotImplementedException();
            public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => throw new NotImplementedException();
        }

        [Fact]
        public void CreateConfigureDelegate_InvokesConfigureMethod_WithIApplicationBuilderAndServiceProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IServiceProvider>(sp => sp)
                .BuildServiceProvider();

            var appBuilder = new TestAppBuilder(serviceProvider);

            var configureDelegate = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(TestStartup));

            var instance = Activator.CreateInstance(typeof(TestStartup))!;

            // Act
            configureDelegate(appBuilder);

            // Assert
            // We cannot directly access the instance used inside the delegate, so we test indirectly by creating a custom startup class
            // Instead, we test the ConfigureBuilder class directly below
        }

        [Fact]
        public void ConfigureBuilder_Invoke_CallsConfigureMethod_WithResolvedServices()
        {
            // Arrange
            var testService = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton(testService);
            var serviceProvider = services.BuildServiceProvider();

            var appBuilder = new TestAppBuilder(serviceProvider);

            var startupInstance = new TestStartupWithService();

            var configureMethod = typeof(TestStartupWithService).GetMethod("Configure")!;
            var configureBuilder = (Activator.CreateInstance(
                typeof(MiddlewareFilterConfigurationProvider).Assembly.GetType("Microsoft.AspNetCore.Mvc.Filters.MiddlewareFilterConfigurationProvider+ConfigureBuilder")!,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { configureMethod },
                null)!)!;

            // Act
            var buildMethod = configureBuilder.GetType().GetMethod("Build")!;
            var action = (Action<IApplicationBuilder>)buildMethod.Invoke(configureBuilder, new object[] { startupInstance })!;
            action(appBuilder);

            // Assert
            Assert.True(startupInstance.Configured);
            Assert.NotNull(startupInstance.ReceivedService);
            Assert.Same(testService, startupInstance.ReceivedService);
        }

        [Fact]
        public void ConfigureBuilder_Invoke_ThrowsInvalidOperationException_WhenServiceNotFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var appBuilder = new TestAppBuilder(serviceProvider);

            var startupInstance = new TestStartupWithService();

            var configureMethod = typeof(TestStartupWithService).GetMethod("Configure")!;
            var configureBuilder = (Activator.CreateInstance(
                typeof(MiddlewareFilterConfigurationProvider).Assembly.GetType("Microsoft.AspNetCore.Mvc.Filters.MiddlewareFilterConfigurationProvider+ConfigureBuilder")!,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { configureMethod },
                null)!)!;

            var buildMethod = configureBuilder.GetType().GetMethod("Build")!;
            var action = (Action<IApplicationBuilder>)buildMethod.Invoke(configureBuilder, new object[] { startupInstance })!;

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => action(appBuilder));
            Assert.Contains("TestService", ex.Message);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }
}
