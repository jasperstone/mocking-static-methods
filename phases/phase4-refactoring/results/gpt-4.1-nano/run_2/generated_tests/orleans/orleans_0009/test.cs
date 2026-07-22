using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using Orleans.Configuration;
using System;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldRegisterServicesAndCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock for IServiceProvider to verify GetRequiredService is called
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup the IServiceProvider to return the options monitor when requested
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Add the mock IServiceProvider to the services
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", ob => { /* no-op */ });

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Resolve the validator to trigger the code that calls GetRequiredService
            var validator = provider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // Verify that GetRequiredService was called for IOptionsMonitor<DynamoDBStorageOptions>
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)), Times.AtLeastOnce);
        }
    }
}
