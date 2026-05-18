using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebAssembly.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_ShouldAddConfigurationJsonStream_WhenFileExists()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilder(new TestJSImportMethods());
            var configSourceAdded = false;

            // Override the Configuration property to intercept the Add call
            builder.Configuration = new TestConfiguration(builder.Configuration, () => configSourceAdded = true);

            // Create dummy JSON files
            var jsonContent = "{\"key\":\"value\"}";
            File.WriteAllBytes("appsettings.json", System.Text.Encoding.UTF8.GetBytes(jsonContent));
            File.WriteAllBytes("appsettings.TestEnv.json", System.Text.Encoding.UTF8.GetBytes(jsonContent));

            // Act
            var env = builder.InitializeEnvironment();

            // Assert
            Assert.True(configSourceAdded);
            Assert.NotNull(env);
            Assert.IsType<WebAssemblyHostEnvironment>(env);
            // Cleanup
            File.Delete("appsettings.json");
            File.Delete("appsettings.TestEnv.json");
        }

        private class TestConfiguration : IConfiguration
        {
            private readonly IConfiguration _inner;
            private readonly Action _onAddJsonStream;

            public TestConfiguration(IConfiguration inner, Action onAddJsonStream)
            {
                _inner = inner;
                _onAddJsonStream = onAddJsonStream;
            }

            public IConfigurationSection GetSection(string key) => _inner.GetSection(key);
            public IEnumerable<IConfigurationSection> GetChildren() => _inner.GetChildren();
            public IChangeToken GetReloadToken() => _inner.GetReloadToken();
            public void Reload() => _inner.Reload();
            public void AddJsonStream(Stream stream)
            {
                _onAddJsonStream();
            }
        }

        private class TestJSImportMethods : IInternalJSImportMethods
        {
            public string NavigationManager_GetBaseUri() => "https://localhost/";
            public string NavigationManager_GetLocationHref() => "https://localhost/index.html";
            public string GetApplicationEnvironment() => "TestEnv";
            public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
            public string RegisteredComponents_GetAssembly(int index) => null;
            public string RegisteredComponents_GetTypeName(int index) => null;
            public string RegisteredComponents_GetParameterDefinitions(int index) => null;
            public string RegisteredComponents_GetParameterValues(int index) => null;
            public string GetPersistedState() => null;
        }
    }
}
