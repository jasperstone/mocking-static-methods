using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration.Json;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFilesExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods("Development", "https://localhost/", "https://localhost/index.html");
            var builder = CreateBuilderWithJsMethods(jsMethods);

            // Create temp config files
            var configFile1 = "appsettings.json";
            var configFile2 = "appsettings.Development.json";

            var configContent1 = "{\"key1\":\"value1\"}";
            var configContent2 = "{\"key2\":\"value2\"}";

            File.WriteAllText(configFile1, configContent1);
            File.WriteAllText(configFile2, configContent2);

            try
            {
                // Act
                var hostEnv = InvokeInitializeEnvironment(builder);

                // Assert
                var jsonStreamSources = builder.Configuration.Sources.OfType<JsonStreamConfigurationSource>().ToList();
                Assert.Equal(2, jsonStreamSources.Count);

                var streamsContent = new List<string>();
                foreach (var source in jsonStreamSources)
                {
                    source.Stream.Position = 0;
                    using var reader = new StreamReader(source.Stream);
                    streamsContent.Add(reader.ReadToEnd());
                }

                Assert.Contains(configContent1, streamsContent);
                Assert.Contains(configContent2, streamsContent);

                Assert.Equal("Development", hostEnv.Environment);
                Assert.Equal("https://localhost/", hostEnv.BaseAddress);
            }
            finally
            {
                if (File.Exists(configFile1)) File.Delete(configFile1);
                if (File.Exists(configFile2)) File.Delete(configFile2);
            }
        }

        [Fact]
        public void InitializeEnvironment_DoesNotAddJsonStreamConfigurationSource_WhenConfigFilesDoNotExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods("Production", "https://example.com/", "https://example.com/index.html");
            var builder = CreateBuilderWithJsMethods(jsMethods);

            // Act
            var hostEnv = InvokeInitializeEnvironment(builder);

            // Assert
            var jsonStreamSources = builder.Configuration.Sources.OfType<JsonStreamConfigurationSource>().ToList();
            Assert.Empty(jsonStreamSources);

            Assert.Equal("Production", hostEnv.Environment);
            Assert.Equal("https://example.com/", hostEnv.BaseAddress);
        }

        private static object CreateBuilderWithJsMethods(object jsMethods)
        {
            var builderType = typeof(WebAssemblyHostBuilder);
            var ctor = builderType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { jsMethods.GetType() }, null);
            if (ctor == null)
            {
                // Try to find constructor with interface type parameter
                var constructors = builderType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var c in constructors)
                {
                    var parameters = c.GetParameters();
                    if (parameters.Length == 1)
                    {
                        return c.Invoke(new[] { jsMethods });
                    }
                }
                throw new InvalidOperationException("Suitable constructor not found");
            }
            return ctor.Invoke(new[] { jsMethods });
        }

        private static WebAssemblyHostEnvironment InvokeInitializeEnvironment(object builder)
        {
            var method = builder.GetType().GetMethod("InitializeEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null) throw new InvalidOperationException("InitializeEnvironment method not found");
            return (WebAssemblyHostEnvironment)method.Invoke(builder, null)!;
        }

        private class TestJSImportMethods
        {
            private readonly string _environment;
            private readonly string _baseUri;
            private readonly string _locationHref;

            public TestJSImportMethods(string environment, string baseUri, string locationHref)
            {
                _environment = environment;
                _baseUri = baseUri;
                _locationHref = locationHref;
            }

            public string GetApplicationEnvironment() => _environment;
            public string NavigationManager_GetBaseUri() => _baseUri;
            public string NavigationManager_GetLocationHref() => _locationHref;
            public string? GetPersistedState() => null;
            public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
        }
    }
}
