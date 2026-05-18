using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp;
using Volo.Abp.Internal.Telemetry;
using Xunit;

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTelemetryTests
    {
        // Derived class inside the Volo.Abp namespace to access internal and protected members
        private class TestAbpApplication : AbpApplicationWithInternalServiceProvider
        {
            public TestAbpApplication(Type startupModuleType, Action<AbpApplicationCreationOptions>? optionsAction)
                : base(startupModuleType, optionsAction)
            {
            }

            public void SetServiceProviderForTest(IServiceProvider serviceProvider)
            {
                SetServiceProvider(serviceProvider);
            }

            public Task CallInitializeTelemetryTrackingAsync()
            {
                var method = typeof(AbpApplicationBase).GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("Method InitializeTelemetryTracking not found");
                return (Task)method.Invoke(this, null)!;
            }
        }

        [Fact]
        public async Task InitializeTelemetryTracking_CallsAddActivityAsync()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var scopedServiceProviderMock = new Mock<IServiceProvider>();
            scopedServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopedServiceProviderMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(typeof(TestAbpApplication), null);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            await app.CallInitializeTelemetryTrackingAsync();

            telemetryServiceMock.Verify(x => x.AddActivityAsync("ApplicationRun"), Times.Once);
        }

        [Fact]
        public async Task InitializeTelemetryTracking_LogsException_WhenTelemetryServiceThrows()
        {
            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var scopedServiceProviderMock = new Mock<IServiceProvider>();
            scopedServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ITelemetryService)))
                .Returns(telemetryServiceMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(scopedServiceProviderMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(serviceScopeMock.Object);

            var loggerMock = new Mock<ILogger<AbpApplicationBase>>();
            loggerMock.Setup(l => l.LogException(It.IsAny<Exception>(), It.IsAny<LogLevel>()));

            var servicesWithLogger = new ServiceCollectionWithGetInitLogger(loggerMock.Object);

            var app = new TestAbpApplication(typeof(TestAbpApplication), null);
            typeof(AbpApplicationBase).GetProperty("Services")!.SetValue(app, servicesWithLogger);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            await app.CallInitializeTelemetryTrackingAsync();

            loggerMock.Verify(l => l.LogException(It.IsAny<Exception>(), LogLevel.Trace), Times.Once);
        }

        private class ServiceCollectionWithGetInitLogger : IServiceCollection
        {
            private readonly ILogger<AbpApplicationBase> _logger;

            public ServiceCollectionWithGetInitLogger(ILogger<AbpApplicationBase> logger)
            {
                _logger = logger;
            }

            public ILogger<AbpApplicationBase> GetInitLogger<T>()
            {
                return _logger;
            }

            #region IServiceCollection members (not used, so throw)

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public ServiceDescriptor this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void Add(ServiceDescriptor item) => throw new NotImplementedException();

            public void Clear() => throw new NotImplementedException();

            public bool Contains(ServiceDescriptor item) => throw new NotImplementedException();

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => throw new NotImplementedException();

            public System.Collections.Generic.IEnumerator<ServiceDescriptor> GetEnumerator() => throw new NotImplementedException();

            public int IndexOf(ServiceDescriptor item) => throw new NotImplementedException();

            public void Insert(int index, ServiceDescriptor item) => throw new NotImplementedException();

            public bool Remove(ServiceDescriptor item) => throw new NotImplementedException();

            public void RemoveAt(int index) => throw new NotImplementedException();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();

            #endregion
        }
    }
}
