using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using System.Collections.Generic;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_Should_Call_GetValue_For_developSelfHosted()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "GlobalSettings:SomeSetting", "value" },
                { "developSelfHosted", "true" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var environment = new TestHostEnvironment { IsDevelopment = () => true };

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
        }

        [Fact]
        public void AddGlobalSettingsServices_Should_Use_Configuration_GetValue_Boolean()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string>
            {
                { "GlobalSettings:SomeSetting", "value" },
                { "developSelfHosted", "true" }
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var environment = new TestHostEnvironment { IsDevelopment = () => true };

            // Act
            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            // Assert
            Assert.NotNull(globalSettings);
        }

        // Helper class to mock IHostEnvironment
        public class TestHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; }
            public Func<bool> IsDevelopment { get; set; }

            public bool IsDevelopment() => IsDevelopment?.Invoke() ?? false;
        }
    }
}
