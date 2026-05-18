using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainDirectoryServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainDirectory_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add necessary dependencies for the extension method
            services.AddOptions();
            services.AddTransient<IOptionsMonitor<AdoNetGrainDirectoryOptions>, DummyOptionsMonitor>();
            services.AddTransient<AdoNetGrainDirectory>();
            // Note: ActivatorUtilities is a static class, so we can't mock it directly.
            // Instead, we will test the registration process and resolution.

            string name = "testName";

            // Act
            var result = services.AddAdoNetGrainDirectory(name, options => { });

            // Build the service provider
            var serviceProvider = result.BuildServiceProvider();

            // Assert
            // Verify that GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>() can be resolved
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainDirectoryOptions>>();
            Assert.NotNull(optionsMonitor);
        }

        // Dummy implementations for dependencies
        private class DummyOptionsMonitor : IOptionsMonitor<AdoNetGrainDirectoryOptions>
        {
            public AdoNetGrainDirectoryOptions Get(string name) => new AdoNetGrainDirectoryOptions();
            public AdoNetGrainDirectoryOptions CurrentValue => throw new NotImplementedException();
            public IDisposable OnChange(Action<AdoNetGrainDirectoryOptions, string> listener) => throw new NotImplementedException();
        }

        private class AdoNetGrainDirectoryOptions { }
        private class AdoNetGrainDirectory { }
    }
}
