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

        private class ConfigWithParameterlessConstructor
        {
            public bool Invoked { get; private set; }
            public void Configure(IApplicationBuilder app, DummyService service)
            {
                Invoked = true;
            }
        }

        private class ConfigWithoutParameterlessConstructor
        {
            public ConfigWithoutParameterlessConstructor(int x) { }
            public void Configure(IApplicationBuilder app, DummyService service)
            {
            }
        }

        private class ConfigWithMultipleConfigureMethods
        {
            public void Configure() { }
            public void Configure(int x) { }
        }

        private class ConfigWithWrongReturnType
        {
            public int Configure() => 0;
        }

        [Fact]
        public void CreateConfigureDelegate_ShouldCreateDelegateAndInvokeMethod()
        {
            // Arrange
            var type = typeof(ConfigWithParameterlessConstructor);
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("Configure");
            var builder = new ApplicationBuilder(new ServiceCollection().BuildServiceProvider());

            // Act
            var delegateBuilder = MiddlewareFilterConfigurationProvider.GetType()
                .GetMethod("GetConfigureDelegateBuilder", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { type }) as dynamic;
            var action = delegateBuilder.Build(instance);
            action(builder);

            // Assert
            Assert.NotNull(action);
        }

        [Fact]
        public void CreateConfigureDelegate_ShouldThrowIfNoParameterlessConstructor()
        {
            // Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(ConfigWithoutParameterlessConstructor))
            );
        }

        [Fact]
        public void CreateConfigureDelegate_ShouldThrowIfMultipleConfigureMethods()
        {
            // Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(ConfigWithMultipleConfigureMethods))
            );
        }

        [Fact]
        public void CreateConfigureDelegate_ShouldThrowIfConfigureMethodHasWrongReturnType()
        {
            // Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(typeof(ConfigWithWrongReturnType))
            );
        }

        [Fact]
        public void Invoke_ShouldResolveServiceAndCallMethod()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddTransient<DummyService>()
                .BuildServiceProvider();

            var appBuilder = new ApplicationBuilder(serviceProvider);
            var methodInfo = typeof(ConfigWithParameterlessConstructor).GetMethod("Configure");
            var instance = new ConfigWithParameterlessConstructor();

            var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
            var action = builder.Build(instance);

            // Act
            action(appBuilder);

            // Assert
            // No exception means success
        }

        [Fact]
        public void Invoke_ShouldThrowIfServiceCannotBeResolved()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);
            var methodInfo = typeof(ConfigWithParameterlessConstructor).GetMethod("Configure");
            var instance = new ConfigWithParameterlessConstructor();

            var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build(instance)(appBuilder));
        }
    }
}
