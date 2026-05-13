using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonConfiguration()
        {
            // Arrange
            var jsMethods = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethods.Object);
            var environment = "Development";
            var baseAddress = "https://example.com";
            var configFile = "appsettings.json";
            var configJson = "{\"key\":\"value\"}";
            File.WriteAllText(configFile, configJson);

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.Equal(environment, hostEnvironment.Environment);
            Assert.Equal(baseAddress, hostEnvironment.BaseAddress);
            Assert.True(File.Exists(configFile));
            File.Delete(configFile);
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonConfiguration_ForEnvironment()
        {
            // Arrange
            var jsMethods = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethods.Object);
            var environment = "Development";
            var baseAddress = "https://example.com";
            var configFile = $"appsettings.{environment}.json";
            var configJson = "{\"key\":\"value\"}";
            File.WriteAllText(configFile, configJson);

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.Equal(environment, hostEnvironment.Environment);
            Assert.Equal(baseAddress, hostEnvironment.BaseAddress);
            Assert.True(File.Exists(configFile));
            File.Delete(configFile);
        }

        [Fact]
        public void InitializeEnvironment_DoesNotAddJsonConfiguration_ForNonExistentFile()
        {
            // Arrange
            var jsMethods = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethods.Object);
            var environment = "Development";
            var baseAddress = "https://example.com";
            var configFile = "nonexistent.json";

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.Equal(environment, hostEnvironment.Environment);
            Assert.Equal(baseAddress, hostEnvironment.BaseAddress);
            Assert.False(File.Exists(configFile));
        }
    }
}
