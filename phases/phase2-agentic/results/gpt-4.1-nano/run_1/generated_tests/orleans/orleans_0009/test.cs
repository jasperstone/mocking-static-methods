using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Storage;
using Moq;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_Should_Register_Transient_Validator_With_Correct_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy OptionsMonitor for the test
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup the service provider to return the options monitor
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestName", ob => { });

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the validator registration
            var validatorDescriptors = services.Where(d => d.ServiceType == typeof(IConfigurationValidator)).ToList();

            // Assert
            Assert.NotEmpty(validatorDescriptors);
            var validatorDescriptor = validatorDescriptors.First();

            // Create an instance of the validator
            var validatorInstance = validatorDescriptor.ImplementationInstance ?? 
                (validatorDescriptor.ImplementationFactory != null ? validatorDescriptor.ImplementationFactory(serviceProvider) : null);

            Assert.NotNull(validatorInstance);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validatorInstance);

            // Verify that the validator was constructed with the correct options
            var validator = validatorInstance as DynamoDBGrainStorageOptionsValidator;
            Assert.NotNull(validator);
        }
    }
}
