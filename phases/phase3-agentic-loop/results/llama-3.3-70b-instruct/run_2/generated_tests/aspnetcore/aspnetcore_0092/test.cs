using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace WebAssemblyHostBuilderTests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilder(new InternalJSImportMethods());
            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Development.json"
            };

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.True(builder.Configuration.Providers.Any(p => p is JsonStreamConfigurationProvider));
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WithExistingConfig()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilder(new InternalJSImportMethods());
            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Development.json"
            };
            var existingConfig = new ConfigurationBuilder().AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("key", "value")
            }).Build();
            builder.Configuration.Add(existingConfig);

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.True(builder.Configuration.Providers.Any(p => p is JsonStreamConfigurationProvider));
        }

        [Fact]
        public void InitializeEnvironment_DoesNotAddJsonStreamConfigurationSource_IfFileDoesNotExist()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilder(new InternalJSImportMethods());
            var configFiles = new[]
            {
                "nonexistent.json"
            };

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.False(builder.Configuration.Providers.Any(p => p is JsonStreamConfigurationProvider));
        }
    }
}
