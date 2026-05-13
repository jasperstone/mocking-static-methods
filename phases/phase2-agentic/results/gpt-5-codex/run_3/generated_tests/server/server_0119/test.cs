using System.Collections.Generic;
using System.IO;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_WhenDevelopmentAndDevelopSelfHostedTrue_UsesDevOverride()
        {
            var services = new ServiceCollection();
            var configValues = new Dictionary<string, string?>
            {
                ["GlobalSettings:DevelopmentDirectory"] = "BaseDir",
                ["Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory"] = "OverrideDir",
                ["developSelfHosted"] = "true",
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            Assert.Equal("OverrideDir", globalSettings.DevelopmentDirectory);

            using var provider = services.BuildServiceProvider();
            var resolvedConcrete = provider.GetRequiredService<GlobalSettings>();
            var resolvedInterface = provider.GetRequiredService<IGlobalSettings>();

            Assert.Same(globalSettings, resolvedConcrete);
            Assert.Same(globalSettings, resolvedInterface);
        }

        [Fact]
        public void AddGlobalSettingsServices_WhenDevelopSelfHostedDisabled_KeepsBaseSettings()
        {
            var services = new ServiceCollection();
            var configValues = new Dictionary<string, string?>
            {
                ["GlobalSettings:DevelopmentDirectory"] = "BaseDir",
                ["Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory"] = "OverrideDir",
                ["developSelfHosted"] = "false",
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

            var globalSettings = services.AddGlobalSettingsServices(configuration, environment);

            Assert.Equal("BaseDir", globalSettings.DevelopmentDirectory);
        }

        private sealed class TestHostEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Production;
            public string ApplicationName { get; set; } = "TestApp";
            public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }
    }
}
