using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersServicesAndValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestName";

            // Act
            // Call the internal extension method via reflection since it's internal
            var method = typeof(AdoNetGrainDirectoryServiceCollectionExtensions)
                .GetMethod("AddAdoNetGrainDirectory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);

            var optionsBuilderAction = new Action<OptionsBuilder<AdoNetGrainDirectoryOptions>>(optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.Invariant = "InvariantTest";
                    options.ConnectionString = "ConnectionStringTest";
                });
            });

            var result = method.Invoke(null, new object[] { services, name, optionsBuilderAction });
            Assert.NotNull(result);

            var provider = services.BuildServiceProvider();

            // Assert
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = optionsMonitor.Get(name);
            Assert.Equal("InvariantTest", options.Invariant);
            Assert.Equal("ConnectionStringTest", options.ConnectionString);

            var validator = provider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);

            var adoNetValidator = (AdoNetGrainDirectoryOptionsValidator)validator;
            adoNetValidator.ValidateConfiguration();
        }
    }
}
