using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.AdoNet.Storage.Provider; // Correct namespace for options and validator

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ShouldRegisterAdoNetGrainStorageOptionsValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainStorage("TestStorage");

            var provider = services.BuildServiceProvider();

            // Assert
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Verify that the validator is of the expected type
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);
        }
    }
}
