using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime.Hosting;
using System;

namespace Orleans.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_Should_Register_And_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup options monitor mock
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            var options = new AdoNetGrainDirectoryOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Add options to services
            services.AddOptions<AdoNetGrainDirectoryOptions>("testName");
            // Register a dummy implementation for GetOptionsByName extension method if needed
            // For now, assume it's an extension method that works with the options

            // Act
            services.AddAdoNetGrainDirectory("testName", opt => { });

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Act: resolve the validator to trigger the GetRequiredService call
            var validator = provider.GetRequiredService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainDirectoryOptionsValidator>(validator);
        }
    }

    // Dummy classes to satisfy dependencies
    public class AdoNetGrainDirectoryOptions { }
    public class AdoNetGrainDirectoryOptionsValidator : IConfigurationValidator
    {
        public AdoNetGrainDirectoryOptionsValidator(AdoNetGrainDirectoryOptions options, string name) { }
        public void Validate() { }
    }
    public interface IConfigurationValidator
    {
        void Validate();
    }
}
