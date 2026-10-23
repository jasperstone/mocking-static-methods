using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.GrainDirectory.AdoNet;
using Orleans.Hosting;
using Xunit;

namespace Orleans.GrainDirectory.AdoNet.Tests
{
    public class AdoNetGrainDirectorySiloBuilderExtensionsTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }
        }

        [Fact]
        public void AddAdoNetGrainDirectory_RegistersValidatorAndOptions()
        {
            // Arrange
            var builder = new TestSiloBuilder();
            var optionsName = "testName";

            // Act
            builder.AddAdoNetGrainDirectory(optionsName, optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.Invariant = "InvariantValue";
                    options.ConnectionString = "ConnectionStringValue";
                });
            });

            var serviceProvider = builder.Services.BuildServiceProvider();

            // Assert
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);

            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = optionsMonitor.Get(optionsName);
            Assert.Equal("InvariantValue", options.Invariant);
            Assert.Equal("ConnectionStringValue", options.ConnectionString);

            var adoNetValidator = Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);
            adoNetValidator.ValidateConfiguration();
        }
    }
}
