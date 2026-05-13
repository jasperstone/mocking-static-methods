using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            public int RegisteredComponentsCount { get; set; } = 0;

            public string GetPersistedStateResult { get; set; } = null!;

            public string NavigationManager_GetBaseUri() => BaseUri;
            public string NavigationManager_GetLocationHref() => LocationHref;
            public string GetApplicationEnvironment() => ApplicationEnvironment;

            public int RegisteredComponents_GetRegisteredComponentsCount() => RegisteredComponentsCount;
            public string RegisteredComponents_GetAssembly(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetTypeName(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetParameterDefinitions(int index) => throw new NotImplementedException();
            public string RegisteredComponents_GetParameterValues(int index) => throw new NotImplementedException();

            public string GetPersistedState() => GetPersistedStateResult;
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFilesExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods();
            var builder = new WebAssemblyHostBuilder(jsMethods);

            // We need to simulate the presence of config files and their content.
            // The InitializeEnvironment method is private, so we use reflection to invoke it.
            var configFiles = new[]
            {
                "appsettings.json",
                $"appsettings.{jsMethods.ApplicationEnvironment}.json"
            };

            // Create dummy config files with some content
            foreach (var configFile in configFiles)
            {
                File.WriteAllText(configFile, "{ \"TestKey\": \"TestValue\" }");
            }

            try
            {
                var method = typeof(WebAssemblyHostBuilder).GetMethod("InitializeEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.NotNull(method);

                // Act
                var hostEnvironment = method.Invoke(builder, null);

                // Assert
                // The Configuration property should have JsonStreamConfigurationSource added for each existing file
                var sources = builder.Configuration.Sources;
                Assert.NotNull(sources);
                // There should be at least two sources added (one for each config file)
                Assert.True(sources.OfType<JsonStreamConfigurationSource>().Count() >= 2);

                // Check that the hostEnvironment is not null and is of expected type
                Assert.NotNull(hostEnvironment);
                Assert.IsType<WebAssemblyHostEnvironment>(hostEnvironment);
            }
            finally
            {
                // Cleanup dummy config files
                foreach (var configFile in configFiles)
                {
                    if (File.Exists(configFile))
                    {
                        File.Delete(configFile);
                    }
                }
            }
        }
    }
}
