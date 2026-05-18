using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;
using System;
using System.IO;
using System.Text;

namespace WebAssemblyHostBuilderTests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);
            var configFiles = new[] { "appsettings.json" };
            var appSettingsJson = Encoding.UTF8.GetBytes("{}");

            // Act
            builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(builder.Configuration);
            Assert.True(builder.Configuration.Providers.Any(p => p.GetType().Name == "JsonStreamConfigurationProvider"));
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WithFile()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);
            var configFiles = new[] { "appsettings.json" };
            var appSettingsJson = Encoding.UTF8.GetBytes("{\"key\":\"value\"}");

            using var stream = new MemoryStream(appSettingsJson);
            File.WriteAllBytes(configFiles[0], appSettingsJson);

            // Act
            builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(builder.Configuration);
            Assert.True(builder.Configuration.Providers.Any(p => p.GetType().Name == "JsonStreamConfigurationProvider"));
            Assert.Equal("value", builder.Configuration["key"]);

            // Clean up
            File.Delete(configFiles[0]);
        }
    }
}
