using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Xunit;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsName = "TestName";

            // Act
            var returnedServices = services.AddAdoNetGrainDirectory(optionsName, optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.Invariant = "TestInvariant";
                    options.ConnectionString = "TestConnectionString";
                });
            });

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the service registrations and the GetRequiredService call
            var serviceProvider = services.BuildServiceProvider();

            // The AddTransient<IConfigurationValidator> should be registered and resolvable
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The options monitor should be registered and return the configured options
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            Assert.NotNull(optionsMonitor);
            var options = optionsMonitor.Get(optionsName);
            Assert.Equal("TestInvariant", options.Invariant);
            Assert.Equal("TestConnectionString", options.ConnectionString);
        }
    }
}
