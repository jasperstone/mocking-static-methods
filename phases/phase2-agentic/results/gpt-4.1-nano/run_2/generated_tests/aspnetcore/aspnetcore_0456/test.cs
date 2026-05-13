using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MiddlewareFilterTests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        private class DummyService { }

        private class ConfigurableStartup
        {
            public bool WasCalled { get; private set; }
            public void Configure(IApplicationBuilder app, DummyService service)
            {
                WasCalled = true;
            }
        }

        private class NoParameterlessConstructor
        {
            public NoParameterlessConstructor(int x) { }
            public void Configure(IApplicationBuilder app, DummyService service) { }
        }

        private class WrongReturnType
        {
            public void Configure(IApplicationBuilder app, DummyService service) { }
        }

        [Fact]
        public void CreateConfigureDelegate_Should_Return_Delegate_That_Invokes_Method()
        {
            // Arrange
            var type = typeof(ConfigurableStartup);
            var delegateAction = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(type);
            var instance = Activator.CreateInstance(type);
            var appBuilder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());

            // Act
            var action = delegateAction;
            action(appBuilder);

            // Assert
            var startupInstance = (ConfigurableStartup)instance;
            Assert.True(startupInstance.WasCalled);
        }

        [Fact]
        public void CreateConfigureDelegate_Should_Throw_If_No_Parameterless_Constructor()
        {
            // Arrange & Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(NoParameterlessConstructor)));

            // Assert
            Assert.Contains("Cannot create type", ex.Message);
        }

        [Fact]
        public void CreateConfigureDelegate_Should_Throw_If_Method_Not_Found()
        {
            // Arrange
            var type = typeof(WrongReturnType);
            // Remove the 'Configure' method to simulate missing method
            var method = type.GetMethod("Configure");
            // Temporarily remove method (simulate by creating a type without it)
            // But since we can't modify the type at runtime, we can test with a type that has no such method
            // So, create a dummy type
            var dummyType = typeof(DummyNoConfigureMethod);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(dummyType));
            Assert.Contains("NoConfigureMethod", ex.Message);
        }

        private class DummyNoConfigureMethod
        {
            public void SomeOtherMethod() { }
        }

        [Fact]
        public void Invoke_Should_Call_Method_With_Correct_Parameters()
        {
            // Arrange
            var startup = new TestStartup();
            var methodInfo = typeof(TestStartup).GetMethod(nameof(TestStartup.Configure));
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());
            var provider = builder.ApplicationServices;

            var providerMock = new ServiceCollection()
                .AddTransient<DummyService>()
                .BuildServiceProvider();

            var providerField = typeof(ApplicationBuilder).GetField("_applicationServices", BindingFlags.NonPublic | BindingFlags.Instance);
            providerField.SetValue(builder, providerMock);

            var configBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
            var action = configBuilder.Build(startup);

            // Act
            action(builder);

            // Assert
            Assert.True(startup.WasCalled);
        }

        private class TestStartup
        {
            public bool WasCalled { get; private set; }
            public void Configure(IApplicationBuilder app, DummyService service)
            {
                WasCalled = true;
            }
        }
    }
}
