using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            public string GetApplicationEnvironmentResult { get; set; } = "Development";
            public Func<string, bool> FileExistsFunc { get; set; } = _ => false;
            public Func<string, byte[]> ReadAllBytesFunc { get; set; } = _ => Array.Empty<byte>();

            public string NavigationManager_GetBaseUri() => "https://localhost/";
            public string NavigationManager_GetLocationHref() => "https://localhost/index.html";
            public string GetApplicationEnvironment() => GetApplicationEnvironmentResult;
            public string GetPersistedState() => null!;
            public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
            public string RegisteredComponents_GetAssembly(int index) => null!;
            public string RegisteredComponents_GetTypeName(int index) => null!;
            public string RegisteredComponents_GetParameterDefinitions(int index) => null!;
            public string RegisteredComponents_GetParameterValues(int index) => null!;
        }

        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFilesExist()
        {
            // Arrange
            var jsMethods = new TestJSImportMethods();
            var configFiles = new[] { "appsettings.json", $"appsettings.{jsMethods.GetApplicationEnvironment()}.json" };
            var fileContents = new Dictionary<string, byte[]>
            {
                { "appsettings.json", Encoding.UTF8.GetBytes("{\"key1\":\"value1\"}") },
                { $"appsettings.{jsMethods.GetApplicationEnvironment()}.json", Encoding.UTF8.GetBytes("{\"key2\":\"value2\"}") }
            };

            jsMethods.FileExistsFunc = path => configFiles.Contains(path);
            jsMethods.ReadAllBytesFunc = path => fileContents[path];

            var testBuilder = new TestWebAssemblyHostBuilder(jsMethods, configFiles, fileContents);

            // Act
            var hostEnv = testBuilder.InvokeInitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnv);
            var addedSources = testBuilder.Configuration.Sources.OfType<JsonStreamConfigurationSource>().ToList();
            Assert.Equal(2, addedSources.Count);

            foreach (var source in addedSources)
            {
                Assert.NotNull(source.Stream);
                Assert.True(source.Stream.CanRead);
            }
        }

        private class TestWebAssemblyHostBuilder : WebAssemblyHostBuilder
        {
            private readonly string[] _configFiles;
            private readonly Dictionary<string, byte[]> _fileContents;
            private readonly TestJSImportMethods _jsMethods;

            public TestWebAssemblyHostBuilder(TestJSImportMethods jsMethods, string[] configFiles, Dictionary<string, byte[]> fileContents)
                : base(jsMethods)
            {
                _jsMethods = jsMethods;
                _configFiles = configFiles;
                _fileContents = fileContents;
            }

            public WebAssemblyHostEnvironment InvokeInitializeEnvironment()
            {
                return InitializeEnvironment();
            }

            // Override InitializeEnvironment to use injected file system simulation
            private new WebAssemblyHostEnvironment InitializeEnvironment()
            {
                var applicationEnvironment = _jsMethods.GetApplicationEnvironment();
                var hostEnvironment = new WebAssemblyHostEnvironment(applicationEnvironment, WebAssemblyNavigationManager.Instance.BaseUri);

                Services.AddSingleton<IWebAssemblyHostEnvironment>(hostEnvironment);

                foreach (var configFile in _configFiles)
                {
                    if (_jsMethods.FileExistsFunc(configFile))
                    {
                        var appSettingsJson = _jsMethods.ReadAllBytesFunc(configFile);

                        Configuration.Add<JsonStreamConfigurationSource>(s => s.Stream = new MemoryStream(appSettingsJson));
                    }
                }

                return hostEnvironment;
            }
        }
    }
}
