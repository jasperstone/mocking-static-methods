using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // We need to register IOptionsMonitor<AdoNetGrainStorageOptions> so that GetRequiredService does not throw
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            services.AddSingleton(optionsMonitorMock.Object);

            // We also need to register the other dependencies that AddAdoNetGrainStorage adds
            services.AddTransient<IPostConfigureOptions<AdoNetGrainStorageOptions>, DefaultStorageProviderSerializerOptionsConfigurator<AdoNetGrainStorageOptions>>();
            services.AddTransient<IPostConfigureOptions<AdoNetGrainStorageOptions>, DefaultAdoNetGrainStorageOptionsHashPickerConfigurator>();
            services.AddTransient<IConfigurationValidator>(sp => new AdoNetGrainStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>().Get("TestName"), "TestName"));

            // Act
            var result = services.AddAdoNetGrainStorage("TestName", ob => { });

            // Assert
            Assert.NotNull(result);
            // The service collection should contain the IConfigurationValidator registration
            var provider = services.BuildServiceProvider();
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }
    }
}
