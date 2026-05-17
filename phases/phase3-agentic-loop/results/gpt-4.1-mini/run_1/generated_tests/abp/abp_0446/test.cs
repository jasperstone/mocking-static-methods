using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Volo.Abp.Internal.Telemetry;
using Volo.Abp.Internal.Telemetry.Constants;
using Xunit;

namespace Volo.Abp
{
    public class TestAbpApplication : AbpApplicationBase
    {
        public TestAbpApplication(IServiceCollection services)
            : base(typeof(TestAbpApplication), services, null)
        {
        }

        public void SetServiceProviderForTest(IServiceProvider serviceProvider)
        {
            SetServiceProvider(serviceProvider);
        }

        public async Task CallInitializeTelemetryTrackingAsync()
        {
            await (Task)typeof(AbpApplicationBase)
                .GetMethod("InitializeTelemetryTracking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, Array.Empty<object>())!;
        }

        public bool CallShouldSendTelemetryData()
        {
            return (bool)typeof(AbpApplicationBase)
                .GetMethod("ShouldSendTelemetryData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, Array.Empty<object>())!;
        }
    }
}

namespace Volo.Abp.Tests
{
    public class AbpApplicationBaseTests
    {
        [Fact]
        public async Task InitializeTelemetryTracking_CallsCreateScopeAndAddActivityAsync()
        {
            // Arrange
            var services = new ServiceCollection();

            var telemetryServiceMock = new Mock<ITelemetryService>();
            telemetryServiceMock.Setup(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(x => x.ServiceProvider)
                .Returns(new ServiceCollection()
                    .AddSingleton(telemetryServiceMock.Object)
                    .BuildServiceProvider());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object)
                .Verifiable();

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderMock.Object);

            // Act
            await app.CallInitializeTelemetryTrackingAsync();

            // Assert
            serviceProviderMock.Verify(x => x.CreateScope(), Times.Once);
            telemetryServiceMock.Verify(x => x.AddActivityAsync(ActivityNameConsts.ApplicationRun), Times.Once);
        }

        [Fact]
        public void ShouldSendTelemetryData_ReturnsFalse_WhenNotDevelopmentOrTelemetryDisabled()
        {
            // Arrange
            var services = new ServiceCollection();

            var abpHostEnvironmentMock = new Mock<IAbpHostEnvironment>();
            abpHostEnvironmentMock.Setup(x => x.IsDevelopment()).Returns(false);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(x => x.GetValue<bool?>("Abp:Telemetry:IsEnabled")).Returns(true);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IAbpHostEnvironment>())
                .Returns(abpHostEnvironmentMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IConfiguration>())
                .Returns(configurationMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceProviderRootMock = new Mock<IServiceProvider>();
            serviceProviderRootMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var app = new TestAbpApplication(services);
            app.SetServiceProviderForTest(serviceProviderRootMock.Object);

            // Act
            var result = app.CallShouldSendTelemetryData();

            // Assert
            Assert.False(result);
        }
    }
}
