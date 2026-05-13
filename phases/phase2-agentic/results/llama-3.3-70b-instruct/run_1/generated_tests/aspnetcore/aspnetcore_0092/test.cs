using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class WebAssemblyHostBuilderTests
    {
        [TestMethod]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource()
        {
            // Arrange
            var jsMethods = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethods.Object);
            var environment = "Development";
            var baseAddress = "https://example.com";
            var configFile = "appsettings.json";
            var configJson = "{\"key\":\"value\"}";
            var fileBytes = Encoding.UTF8.GetBytes(configJson);

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.IsNotNull(hostEnvironment);
            Assert.AreEqual(environment, hostEnvironment.Environment);
            Assert.AreEqual(baseAddress, hostEnvironment.BaseAddress);

            // Verify that the JsonStreamConfigurationSource is added to the configuration
            var configuration = builder.Configuration;
            var sources = configuration.Sources;
            Assert.IsTrue(sources.Count > 0);
            var jsonStreamSource = sources[0] as JsonStreamConfigurationSource;
            Assert.IsNotNull(jsonStreamSource);
            Assert.IsNotNull(jsonStreamSource.Stream);
            var streamReader = new StreamReader(jsonStreamSource.Stream);
            var streamJson = streamReader.ReadToEnd();
            Assert.AreEqual(configJson, streamJson);
        }
    }
}
