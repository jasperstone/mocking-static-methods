using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Xunit;
using Moq;

namespace Orleans.Tests.Hosting
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup options configuration action
            Action<OptionsBuilder<AdoNetGrainDirectoryOptions>> configureOptions = optionsBuilder =>
            {
                optionsBuilder.Configure(opts =>
                {
                    opts.Invariant = "TestInvariant";
                    opts.ConnectionString = "TestConnectionString";
                });
            };

            // Act
            var returnedServices = services.AddAdoNetGrainDirectory("testName", configureOptions);

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the transient registration and GetRequiredService call
            var serviceProvider = services.BuildServiceProvider();

            // The AddTransient<IConfigurationValidator> should be registered
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The validator should be of type AdoNetGrainDirectoryOptionsValidator
            Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);
        }
    }
}
