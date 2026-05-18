using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests;

public class WebAssemblyHostBuilderConfigurationTests
{
    [Fact]
    public void InitializeEnvironment_AddsJsonStreamConfigurationSource_ForExistingConfigFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDir);

            var appsettingsPath = Path.Combine(tempDir, "appsettings.json");
            var appsettingsDevPath = Path.Combine(tempDir, "appsettings.Development.json");
            
            var jsonContent = "{\"Test\": \"Value\"}";
            File.WriteAllBytes(appsettingsPath, Encoding.UTF8.GetBytes(jsonContent));
            File.WriteAllBytes(appsettingsDevPath, Encoding.UTF8.GetBytes(jsonContent));
            
            var mockJsMethods = new Mock<IInternalJSImportMethods>();
            mockJsMethods.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            mockJsMethods.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");
            mockJsMethods.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/");

            // Act
            var builder = new WebAssemblyHostBuilder(mockJsMethods.Object);

            // Assert
            var jsonSources = builder.Configuration.Sources
                .OfType<JsonStreamConfigurationSource>()
                .ToList();
            
            Assert.Equal(2, jsonSources.Count);

            // Verify first source (appsettings.json)
            using var stream1 = jsonSources[0].Stream!;
            stream1.Position = 0;
            using var reader1 = new StreamReader(stream1);
            Assert.Equal(jsonContent, reader1.ReadToEnd());

            // Verify second source (appsettings.Development.json)
            using var stream2 = jsonSources[1].Stream!;
            stream2.Position = 0;
            using var reader2 = new StreamReader(stream2);
            Assert.Equal(jsonContent, reader2.ReadToEnd());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void InitializeEnvironment_AddsOnlyAppsettingsJson_WhenEnvironmentSpecificFileDoesNotExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDir);

            var appsettingsPath = Path.Combine(tempDir, "appsettings.json");
            var jsonContent = "{\"Test\": \"Value\"}";
            File.WriteAllBytes(appsettingsPath, Encoding.UTF8.GetBytes(jsonContent));

            var mockJsMethods = new Mock<IInternalJSImportMethods>();
            mockJsMethods.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            mockJsMethods.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");
            mockJsMethods.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/");

            // Act
            var builder = new WebAssemblyHostBuilder(mockJsMethods.Object);

            // Assert
            var jsonSources = builder.Configuration.Sources
                .OfType<JsonStreamConfigurationSource>()
                .ToList();
            
            Assert.Single(jsonSources);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void InitializeEnvironment_SkipsConfigurationSources_WhenNoConfigFilesExist()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        var originalDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDir);

            var mockJsMethods = new Mock<IInternalJSImportMethods>();
            mockJsMethods.Setup(m => m.GetApplicationEnvironment()).Returns("Production");
            mockJsMethods.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");
            mockJsMethods.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/");

            // Act
            var builder = new WebAssemblyHostBuilder(mockJsMethods.Object);

            // Assert
            Assert.Empty(builder.Configuration.Sources.OfType<JsonStreamConfigurationSource>());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
