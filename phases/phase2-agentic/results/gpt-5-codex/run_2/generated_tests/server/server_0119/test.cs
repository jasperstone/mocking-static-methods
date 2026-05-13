using System;
using System.Collections.Generic;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace SharedWeb.Tests.Utilities;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlobalSettingsServices_AppliesSelfHostedOverridesInDevelopment()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GlobalSettings:DevelopmentDirectory"] = "base",
                ["developSelfHosted"] = "true",
                ["Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory"] = "override"
            })
            .Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

        var result = services.AddGlobalSettingsServices(configuration, environment);

        Assert.Equal("override", result.DevelopmentDirectory);

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<GlobalSettings>();

        Assert.Same(result, resolved);
    }

    [Fact]
    public void AddGlobalSettingsServices_DoesNotApplyOverrides_WhenDevelopSelfHostedDisabled()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GlobalSettings:DevelopmentDirectory"] = "base",
                ["Dev:SelfHostOverride:GlobalSettings:DevelopmentDirectory"] = "override"
            })
            .Build();
        var environment = new TestHostEnvironment { EnvironmentName = Environments.Development };

        var result = services.AddGlobalSettingsServices(configuration, environment);

        Assert.Equal("base", result.DevelopmentDirectory);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = nameof(ServiceCollectionExtensionsTests);

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
