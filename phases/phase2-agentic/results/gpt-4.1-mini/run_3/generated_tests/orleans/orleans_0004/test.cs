using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.GrainDirectory.AdoNet;
using Xunit;
using Moq;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = new AdoNetGrainDirectoryOptions
            {
                Invariant = "TestInvariant",
                ConnectionString = "TestConnectionString"
            };
            optionsMonitorMock.Setup(m => m.Get("TestName")).Returns(options);

            // Setup a service provider that returns the mocked IOptionsMonitor when requested
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainDirectory("TestName", optionsBuilder =>
            {
                // No additional configuration needed for this test
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // The AddTransient registration for IConfigurationValidator should resolve correctly
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The validator should be of type AdoNetGrainDirectoryOptionsValidator
            Assert.Equal("TestName", ((dynamic)validator).Name);

            // The options used in the validator should be the same as the mocked options
            var validatorOptions = ((dynamic)validator).Options;
            Assert.Equal(options.Invariant, validatorOptions.Invariant);
            Assert.Equal(options.ConnectionString, validatorOptions.ConnectionString);
        }
    }
}
