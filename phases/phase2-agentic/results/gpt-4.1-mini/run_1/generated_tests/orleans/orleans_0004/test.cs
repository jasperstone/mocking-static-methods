using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Xunit;
using Moq;

namespace Orleans.Tests.Hosting
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_RegistersValidatorWithCorrectOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsName = "TestName";

            // Setup options monitor mock
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var optionsInstance = new AdoNetGrainDirectoryOptions();
            optionsMonitorMock.Setup(m => m.Get(optionsName)).Returns(optionsInstance);

            // Register the mock IOptionsMonitor in the service collection
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddAdoNetGrainDirectory(optionsName, optionsBuilder => { });

            // Build service provider to resolve services
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);

            // Validate that the validator was constructed with the expected options and name
            var typedValidator = (AdoNetGrainDirectoryOptionsValidator)validator;
            Assert.Equal(optionsInstance, typedValidator.Options);
            Assert.Equal(optionsName, typedValidator.Name);
        }
    }

    // Minimal stub classes to allow compilation of the test
    internal class AdoNetGrainDirectoryOptions
    {
    }

    internal interface IConfigurationValidator
    {
    }

    internal class AdoNetGrainDirectoryOptionsValidator : IConfigurationValidator
    {
        public AdoNetGrainDirectoryOptions Options { get; }
        public string Name { get; }

        public AdoNetGrainDirectoryOptionsValidator(AdoNetGrainDirectoryOptions options, string name)
        {
            Options = options;
            Name = name;
        }
    }
}
