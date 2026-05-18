using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
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
            var optionName = "TestName";

            // Act
            services.AddOptions<AdoNetGrainDirectoryOptions>(optionName)
                .Configure(options =>
                {
                    options.Invariant = "TestInvariant";
                    options.ConnectionString = "TestConnectionString";
                });

            services.AddTransient<IConfigurationValidator>(sp =>
                new AdoNetGrainDirectoryOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>().Get(optionName), optionName));

            var provider = services.BuildServiceProvider();

            // Assert
            // The service provider should be able to resolve IConfigurationValidator
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The validator should be of type IConfigurationValidator (interface)
            Assert.IsAssignableFrom<IConfigurationValidator>(validator);

            // The validator should have the options with the correct name
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = optionsMonitor.Get(optionName);
            Assert.Equal("TestInvariant", options.Invariant);
            Assert.Equal("TestConnectionString", options.ConnectionString);
        }
    }
}
