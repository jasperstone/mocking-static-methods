using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
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

            // Setup a mock for IServiceProvider to test GetRequiredService
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();

            // Setup the mock to return options when Get method is called with the name
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup the IServiceProvider mock to return the optionsMonitorMock object
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Add the mock IServiceProvider to the services
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", ob => { });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the validator to trigger the code that calls GetRequiredService
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
        }
    }
}
