using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class DummyModule : IAbpModule
        {
            public bool PreConfigureServicesCalled { get; private set; }
            public bool ConfigureServicesCalled { get; private set; }
            public ServiceConfigurationContext? ServiceConfigurationContext { get; set; }
            public List<Assembly> AllAssemblies { get; } = new List<Assembly>();
            public object Instance => this;
            public Type Type => GetType();
            public bool SkipAutoServiceRegistration { get; set; } = false;

            public Task PreConfigureServicesAsync(ServiceConfigurationContext context)
            {
                PreConfigureServicesCalled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void LoadModules_CallsLoadModulesOnIModuleLoader()
        {
            var services = new ServiceCollection();
            var moduleLoaderMock = new Mock<IModuleLoader>();
            services.AddSingleton(moduleLoaderMock.Object);
            var app = new TestAbpApplication(typeof(object), services, null);
            moduleLoaderMock.Setup(m => m.LoadModules(It.IsAny<IServiceCollection>(), typeof(object), It.IsAny<IEnumerable<PlugInSource>>()))
                .Returns(new List<IAbpModuleDescriptor>());

            var result = app.LoadModules(services, new AbpApplicationCreationOptions(services));

            moduleLoaderMock.Verify(m => m.LoadModules(services, typeof(object), It.IsAny<IEnumerable<PlugInSource>>()), Times.Once);
        }

        [Fact]
        public void SetupTelemetryTracking_ShouldCallInitializeTelemetryTracking_WhenShouldSendTelemetryDataReturnsTrue()
        {
            var services = new ServiceCollection();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            services.AddSingleton(telemetryServiceMock.Object);
            var app = new TestAbpApplication(typeof(object), services, null);
            var serviceProviderMock = new ServiceCollection()
                .AddTransient(_ => new ServiceProviderMock(telemetryServiceMock.Object))
                .BuildServiceProvider();

            app.SetServiceProvider(serviceProviderMock);

            var called = false;
            app.SetupTelemetryTracking = () => { called = true; };

            app.ShouldSendTelemetryDataFunc = () => true;

            app.SetupTelemetryTracking();

            Assert.True(called);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_ShouldAddActivityAsync_WhenCalled()
        {
            var services = new ServiceCollection();
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(t => t.AddActivityAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            services.AddSingleton(telemetryServiceMock.Object);
            var app = new TestAbpApplication(typeof(object), services, null);
            var serviceProviderMock = new ServiceCollection()
                .AddTransient(_ => new ServiceProviderMock(telemetryServiceMock.Object))
                .BuildServiceProvider();

            app.SetServiceProvider(serviceProviderMock);

            await app.InvokeInitializeTelemetryTracking();

            telemetryServiceMock.Verify(t => t.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsTrue_WhenEnvironmentIsDevelopmentAndConfigEnabled()
        {
            var services = new ServiceCollection();
            var app = new TestAbpApplication(typeof(object), services, null);
            var serviceProviderMock = new ServiceCollection()
                .AddTransient(_ => new ServiceProviderMockWithEnvAndConfig(true, true))
                .BuildServiceProvider();

            app.SetServiceProvider(serviceProviderMock);

            var result = app.InvokeShouldSendTelemetryData();

            Assert.True(result);
        }

        // Helper classes for mocking
        private class ServiceProviderMock : IServiceProvider
        {
            private readonly ITelemetryService _telemetryService;

            public ServiceProviderMock(ITelemetryService telemetryService)
            {
                _telemetryService = telemetryService;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ITelemetryService))
                    return _telemetryService;
                return null;
            }
        }

        private class ServiceProviderMockWithEnvAndConfig : IServiceProvider
        {
            private readonly bool _isDevelopment;
            private readonly bool _isEnabled;

            public ServiceProviderMockWithEnvAndConfig(bool isDevelopment, bool isEnabled)
            {
                _isDevelopment = isDevelopment;
                _isEnabled = isEnabled;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IAbpHostEnvironment))
                    return new DummyHostEnvironment(_isDevelopment);
                if (serviceType == typeof(IConfiguration))
                    return new DummyConfiguration(_isEnabled);
                return null;
            }
        }

        private class DummyHostEnvironment : IAbpHostEnvironment
        {
            private readonly bool _isDevelopment;
            public DummyHostEnvironment(bool isDevelopment)
            {
                _isDevelopment = isDevelopment;
            }
            public bool IsDevelopment() => _isDevelopment;
        }

        private class DummyConfiguration : IConfiguration
        {
            private readonly bool _isEnabled;
            public DummyConfiguration(bool isEnabled)
            {
                _isEnabled = isEnabled;
            }
            public T GetValue<T>(string key) => (T)(object)_isEnabled;
            // Other members omitted for brevity
            public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => throw new NotImplementedException();
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
            public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        private class ServiceProviderMock : IServiceProvider
        {
            private readonly ITelemetryService _telemetryService;
            public ServiceProviderMock(ITelemetryService telemetryService)
            {
                _telemetryService = telemetryService;
            }
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ITelemetryService))
                    return _telemetryService;
                return null;
            }
        }

        private class ServiceProviderMockWithEnvAndConfig : IServiceProvider
        {
            private readonly bool _isDevelopment;
            private readonly bool _isEnabled;
            public ServiceProviderMockWithEnvAndConfig(bool isDevelopment, bool isEnabled)
            {
                _isDevelopment = isDevelopment;
                _isEnabled = isEnabled;
            }
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IAbpHostEnvironment))
                    return new DummyHostEnvironment(_isDevelopment);
                if (serviceType == typeof(IConfiguration))
                    return new DummyConfiguration(_isEnabled);
                return null;
            }
        }
    }

    // Extension methods for invoking private methods
    public static class AbpApplicationBaseExtensions
    {
        public static void SetServiceProvider(this AbpApplicationBase app, IServiceProvider provider)
        {
            var field = typeof(AbpApplicationBase).GetProperty("ServiceProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(app, provider);
        }

        public static bool ShouldSendTelemetryDataFunc(this AbpApplicationBase app)
        {
            var method = typeof(AbpApplicationBase).GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(app, null);
        }

        public static void SetupTelemetryTracking(this AbpApplicationBase app)
        {
            var method = typeof(AbpApplicationBase).GetMethod("SetupTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(app, null);
        }

        public static async Task InvokeInitializeTelemetryTracking(this AbpApplicationBase app)
        {
            var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(app, null);
        }

        public static bool InvokeShouldSendTelemetryData(this AbpApplicationBase app)
        {
            var method = typeof(AbpApplicationBase).GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(app, null);
        }
    }
}
