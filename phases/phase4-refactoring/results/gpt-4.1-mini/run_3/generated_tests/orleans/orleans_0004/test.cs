using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Orleans.Hosting;
using Xunit;
using Moq;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }

            // Implement the Configuration property with a dummy IServiceProvider to satisfy interface
            IServiceProvider ISiloBuilder.Configuration => new ServiceCollection().BuildServiceProvider();
        }

        [Fact]
        public void AddAdoNetGrainDirectory_RegistersExpectedServicesIncludingValidator()
        {
            // Arrange
            var builder = new TestSiloBuilder();
            var optionsName = "TestGrainDirectory";

            // Act
            builder.AddAdoNetGrainDirectory(optionsName, optionsBuilder =>
            {
                optionsBuilder.Configure(options => options.Invariant = "TestInvariant");
            });

            var serviceProvider = builder.Services.BuildServiceProvider();

            // Assert
            // The options monitor for AdoNetGrainDirectoryOptions should be resolvable
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            Assert.NotNull(optionsMonitor);

            // The transient IConfigurationValidator should be resolvable
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }
    }
}
