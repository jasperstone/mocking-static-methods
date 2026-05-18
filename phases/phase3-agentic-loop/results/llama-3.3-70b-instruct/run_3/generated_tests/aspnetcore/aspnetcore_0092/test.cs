using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonConfigurationSource()
        {
            // Arrange
            var jsMethods = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethods.Object);
            var applicationEnvironment = "Development";
            var baseUri = "https://example.com";
            var configFile = "appsettings.json";
            var configJson = "{\"key\":\"value\"}";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configJson));

            jsMethods.Setup(j => j.GetApplicationEnvironment()).Returns(applicationEnvironment);
            jsMethods.Setup(j => j.NavigationManager_GetBaseUri()).Returns(baseUri);
            jsMethods.Setup(j => j.NavigationManager_GetLocationHref()).Returns("https://example.com/location");

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.Equal(applicationEnvironment, hostEnvironment.EnvironmentName);
            Assert.Equal(baseUri, hostEnvironment.BaseUri);

            // Verify that the Add method was called with a JsonStreamConfigurationSource
            var configurationBuilder = (IConfigurationBuilder)builder.Configuration;
            var sources = configurationBuilder.Sources;
            Assert.Single(sources);
            var source = sources[0];
            Assert.IsType<JsonStreamConfigurationSource>(source);
            var jsonSource = (JsonStreamConfigurationSource)source;
            Assert.Equal(memoryStream, jsonSource.Stream);
        }
    }
}
