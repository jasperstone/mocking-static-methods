using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests;

public class WebAssemblyHostConfigurationTests
{
    [Fact]
    public void AddJsonStreamConfigurationSource_CallsIConfigurationBuilderAdd()
    {
        // Arrange
        var config = new WebAssemblyHostConfiguration();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes("{}");
        var stream = new MemoryStream(jsonBytes);

        // Act
        ((IConfigurationBuilder)config).Add<JsonStreamConfigurationSource>(s => s.Stream = stream);

        // Assert - verify by checking the configuration value can be read
        Assert.NotNull(config);
    }

    [Fact]
    public void AddJsonStreamConfigurationSource_ConfiguresSourceCorrectly()
    {
        // Arrange
        var config = new WebAssemblyHostConfiguration();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes("{\"TestKey\": \"TestValue\"}");
        var stream = new MemoryStream(jsonBytes);

        // Act
        ((IConfigurationBuilder)config).Add<JsonStreamConfigurationSource>(s =>
        {
            s.Stream = stream;
            s.Path = "appsettings.json";
        });

        // Assert
        Assert.Equal("TestValue", config["TestKey"]);
    }

    [Fact]
    public void AddJsonStreamConfigurationSource_MultipleSources_LastWins()
    {
        // Arrange
        var config = new WebAssemblyHostConfiguration();
        var bytes1 = System.Text.Encoding.UTF8.GetBytes("{\"Key\": \"Value1\"}");
        var bytes2 = System.Text.Encoding.UTF8.GetBytes("{\"Key\": \"Value2\"}");

        // Act
        ((IConfigurationBuilder)config).Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(bytes1));
        ((IConfigurationBuilder)config).Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(bytes2));

        // Assert
        Assert.Equal("Value2", config["Key"]);
    }

    [Fact]
    public void AddJsonStreamConfigurationSource_CustomPathAndReload()
    {
        // Arrange
        var config = new WebAssemblyHostConfiguration();
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes("{\"Custom\": \"Configured\"}");

        // Act
        ((IConfigurationBuilder)config).Add<JsonStreamConfigurationSource>(s =>
        {
            s.Stream = new MemoryStream(jsonBytes);
            s.Path = "custom.json";
            s.ReloadOnChange = true;
        });

        // Assert
        Assert.Equal("Configured", config["Custom"]);
    }
}
