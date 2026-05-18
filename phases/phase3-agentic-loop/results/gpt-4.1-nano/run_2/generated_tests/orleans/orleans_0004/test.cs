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

            // Add necessary services for the extension method
            services.AddOptions();
            services.AddTransient<IConfigurationValidator, DummyValidator>();
            services.AddTransient<IOptionsMonitor<AdoNetGrainDirectoryOptions>, DummyOptionsMonitor>();
            services.AddTransient<AdoNetGrainDirectory>();
            services.AddTransient<ActivatorUtilities>();

            string name = "testName";

            // Act
            var result = services.AddAdoNetGrainDirectory(name, options => { });

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, s => s.ServiceType == typeof(AdoNetGrainDirectory));
        }

        // Dummy implementations for dependencies
        private class DummyValidator : IConfigurationValidator { }
        private class DummyOptionsMonitor : IOptionsMonitor<AdoNetGrainDirectoryOptions>
        {
            public AdoNetGrainDirectoryOptions Get(string name) => new AdoNetGrainDirectoryOptions();
            public AdoNetGrainDirectoryOptions Get() => throw new NotImplementedException();
            public IDisposable OnChange(Action<AdoNetGrainDirectoryOptions, string> listener) => throw new NotImplementedException();
        }
    }
}
