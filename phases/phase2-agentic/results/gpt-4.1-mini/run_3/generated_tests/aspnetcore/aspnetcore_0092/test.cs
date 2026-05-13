using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFileExists()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>(MockBehavior.Strict);
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("https://localhost/");
            jsMethodsMock.Setup(m => m.NavigationManager_GetLocationHref()).Returns("https://localhost/index.html");
            jsMethodsMock.Setup(m => m.RegisteredComponents_GetRegisteredComponentsCount()).Returns(0);
            jsMethodsMock.Setup(m => m.GetPersistedState()).Returns((string?)null);

            // Create temporary config files
            var appSettingsJson = "{\"key\":\"value\"}";
            var appSettingsEnvJson = "{\"envKey\":\"envValue\"}";

            File.WriteAllText("appsettings.json", appSettingsJson);
            File.WriteAllText("appsettings.Development.json", appSettingsEnvJson);

            try
            {
                var builder = new TestWebAssemblyHostBuilder(jsMethodsMock.Object);

                // Act
                var hostEnvironment = builder.InvokeInitializeEnvironment();

                // Assert
                Assert.NotNull(hostEnvironment);
                Assert.Equal("Development", hostEnvironment.Environment);
                Assert.Equal("https://localhost/", hostEnvironment.BaseAddress);

                // The Configuration should have two sources added (one for each config file)
                var sources = builder.Configuration.Sources;
                Assert.NotNull(sources);
                Assert.Equal(2, sources.Count);

                // The sources should be JsonStreamConfigurationSource with streams containing the JSON content
                foreach (var source in sources)
                {
                    Assert.IsType<JsonStreamConfigurationSource>(source);
                    var jsonSource = (JsonStreamConfigurationSource)source;
                    Assert.NotNull(jsonSource.Stream);
                    using var reader = new StreamReader(jsonSource.Stream, Encoding.UTF8, true, 1024, leaveOpen: true);
                    jsonSource.Stream.Position = 0;
                    var content = reader.ReadToEnd();
                    Assert.True(content.Contains("key") || content.Contains("envKey"));
                }
            }
            finally
            {
                // Cleanup
                File.Delete("appsettings.json");
                File.Delete("appsettings.Development.json");
            }
        }

        private class TestWebAssemblyHostBuilder : WebAssemblyHostBuilder
        {
            public TestWebAssemblyHostBuilder(IInternalJSImportMethods jsMethods) : base(jsMethods)
            {
            }

            public new WebAssemblyHostConfiguration Configuration => base.Configuration;

            public WebAssemblyHostEnvironment InvokeInitializeEnvironment()
            {
                // Call the private InitializeEnvironment method via reflection
                var method = typeof(WebAssemblyHostBuilder).GetMethod("InitializeEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null) throw new InvalidOperationException("InitializeEnvironment method not found");
                return (WebAssemblyHostEnvironment)method.Invoke(this, null)!;
            }
        }
    }
}
