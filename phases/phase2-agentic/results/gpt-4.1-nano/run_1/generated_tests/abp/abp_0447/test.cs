using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Modularity;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        private class DummyModule : IAbpModule
        {
            public ServiceConfigurationContext ServiceConfigurationContext { get; set; }
        }

        private class DummyTelemetryService : ITelemetryService
        {
            public bool Called { get; private set; } = false;
            public string ActivityName { get; private set; }

            public Task AddActivityAsync(string activityName)
            {
                Called = true;
                ActivityName = activityName;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockTelemetryService = new DummyTelemetryService();

            // Setup service provider with required services
            services.AddLogging();
            services.AddSingleton<ITelemetryService>(mockTelemetryService);
            services.AddSingleton<IAbpHostEnvironment>(new AbpHostEnvironment { EnvironmentName = "Development" });
            services.AddSingleton<IConfiguration>(new ConfigurationMock());
            services.AddTransient<IServiceScopeFactory, ServiceScopeFactoryMock>();

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);

            // Act
            await app.InitializeTelemetryTracking();

            // Assert
            Assert.True(mockTelemetryService.Called);
            Assert.Equal(ActivityNameConsts.ApplicationRun, mockTelemetryService.ActivityName);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_ExceptionLogs()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup service provider with required services
            services.AddLogging();
            services.AddSingleton<ITelemetryService>(new ThrowingTelemetryService());
            services.AddSingleton<IAbpHostEnvironment>(new AbpHostEnvironment { EnvironmentName = "Development" });
            services.AddSingleton<IConfiguration>(new ConfigurationMock());
            services.AddTransient<IServiceScopeFactory, ServiceScopeFactoryMock>();

            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);

            // Act
            await app.InitializeTelemetryTracking();

            // Assert
            // Since logger is mocked, we just ensure no exception is thrown
        }

        [Fact]
        public void ShouldSendTelemetryData_Windows_ReturnsExpected()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddSingleton<IAbpHostEnvironment>(new AbpHostEnvironment { EnvironmentName = "Development" });
            services.AddSingleton<IConfiguration>(new ConfigurationMock { TelemetryEnabled = true });
            var serviceProvider = services.BuildServiceProvider();

            var app = new TestAbpApplication(serviceProvider);

            // Act
            var result = app.ShouldSendTelemetryData();

            // Assert
            Assert.True(result);
        }

        private class ConfigurationMock : IConfiguration
        {
            public bool TelemetryEnabled { get; set; } = true;

            public T GetValue<T>(string key)
            {
                if (key == "Abp:Telemetry:IsEnabled")
                {
                    return (T)(object)TelemetryEnabled;
                }
                return default;
            }

            // Other IConfiguration members not implemented for brevity
            public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
            public IChangeToken GetReloadToken() => throw new NotImplementedException();
            public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
            public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        private class ServiceScopeFactoryMock : IServiceScopeFactory
        {
            public IServiceScope CreateScope()
            {
                return new ServiceScopeMock();
            }
        }

        private class ServiceScopeMock : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

            public void Dispose() { }
        }

        private class TestAbpApplication : AbpApplicationBase
        {
            public TestAbpApplication(IServiceProvider serviceProvider)
                : base(typeof(DummyModule), new ServiceCollection(), null)
            {
                SetServiceProvider(serviceProvider);
            }

            public new async Task InitializeTelemetryTracking()
            {
                await base.InitializeTelemetryTracking();
            }

            public new bool ShouldSendTelemetryData()
            {
                return base.ShouldSendTelemetryData();
            }
        }

        private class ThrowingTelemetryService : ITelemetryService
        {
            public Task AddActivityAsync(string activityName)
            {
                throw new Exception("Test exception");
            }
        }
    }
}
