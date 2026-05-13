using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        private class TestJSImportMethods : IInternalJSImportMethods
        {
            public string BaseUri { get; set; } = "https://localhost/";
            public string LocationHref { get; set; } = "https://localhost/index.html";
            public string ApplicationEnvironment { get; set; } = "Development";
            public string PersistedState { get; set; } = null!;
            public int RegisteredComponentsCount { get; set; } = 0;

            public string NavigationManager_GetBaseUri() => BaseUri;
            public string NavigationManager_GetLocationHref() => LocationHref;
            public string GetApplicationEnvironment() => ApplicationEnvironment;
            public string GetPersistedState() => PersistedState;
            public int RegisteredComponents_GetRegisteredComponentsCount() => RegisteredComponentsCount;

            public string RegisteredComponents_GetAssembly(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetTypeName(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetParameterDefinitions(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetParameterValues(int index) => throw new NotImplementedException();
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFilesExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods();
            var builder = new WebAssemblyHostBuilder(jsMethods);

            // Setup temporary config files
            var configFile1 = "appsettings.json";
            var configFile2 = $"appsettings.{jsMethods.ApplicationEnvironment}.json";

            var configContent1 = "{\"key1\":\"value1\"}";
            var configContent2 = "{\"key2\":\"value2\"}";

            File.WriteAllText(configFile1, configContent1);
            File.WriteAllText(configFile2, configContent2);

            try
            {
                // Act
                var hostEnvironment = InvokeInitializeEnvironment(builder);

                // Assert
                // The Configuration should contain two JsonStreamConfigurationSource entries
                var jsonStreamSources = builder.Configuration.Sources.OfType<JsonStreamConfigurationSource>().ToList();
                Assert.Equal(2, jsonStreamSources.Count);

                // Validate the streams contain the expected JSON content
                using (var reader1 = new StreamReader(jsonStreamSources[0].Stream))
                {
                    var content = reader1.ReadToEnd();
                    Assert.Contains("key1", content);
                }
                using (var reader2 = new StreamReader(jsonStreamSources[1].Stream))
                {
                    var content = reader2.ReadToEnd();
                    Assert.Contains("key2", content);
                }

                // Validate host environment properties
                Assert.Equal(jsMethods.ApplicationEnvironment, hostEnvironment.Environment);
                Assert.Equal(jsMethods.BaseUri, hostEnvironment.BaseAddress);
            }
            finally
            {
                // Cleanup
                if (File.Exists(configFile1))
                {
                    File.Delete(configFile1);
                }
                if (File.Exists(configFile2))
                {
                    File.Delete(configFile2);
                }
            }
        }

        [Fact]
        public void InitializeEnvironment_DoesNotAddConfigurationSource_WhenConfigFilesDoNotExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods();
            var builder = new WebAssemblyHostBuilder(jsMethods);

            // Ensure config files do not exist
            var configFile1 = "appsettings.json";
            var configFile2 = $"appsettings.{jsMethods.ApplicationEnvironment}.json";

            if (File.Exists(configFile1)) File.Delete(configFile1);
            if (File.Exists(configFile2)) File.Delete(configFile2);

            // Act
            var hostEnvironment = InvokeInitializeEnvironment(builder);

            // Assert
            var jsonStreamSources = builder.Configuration.Sources.OfType<JsonStreamConfigurationSource>().ToList();
            Assert.Empty(jsonStreamSources);

            Assert.Equal(jsMethods.ApplicationEnvironment, hostEnvironment.Environment);
            Assert.Equal(jsMethods.BaseUri, hostEnvironment.BaseAddress);
        }

        private static WebAssemblyHostEnvironment InvokeInitializeEnvironment(WebAssemblyHostBuilder builder)
        {
            // Use reflection to invoke the private InitializeEnvironment method
            var method = typeof(WebAssemblyHostBuilder).GetMethod("InitializeEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var result = method.Invoke(builder, null);
            Assert.IsType<WebAssemblyHostEnvironment>(result);
            return (WebAssemblyHostEnvironment)result!;
        }
    }
}
