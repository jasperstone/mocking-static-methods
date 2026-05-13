using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebAssemblyHostBuilderTests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_ShouldCallConfigurationAddJsonStream_ForExistingFiles()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilderStub();

            // Create dummy JSON files
            var jsonContent1 = "{\"key\":\"value\"}";
            var jsonContent2 = "{\"env\":\"prod\"}";
            File.WriteAllBytes("appsettings.json", System.Text.Encoding.UTF8.GetBytes(jsonContent1));
            File.WriteAllBytes("appsettings.Production.json", System.Text.Encoding.UTF8.GetBytes(jsonContent2));

            // Act
            var environment = builder.InvokeInitializeEnvironment();

            // Assert
            Assert.Contains(builder.ConfigurationStreamContents, content => System.Text.Encoding.UTF8.GetString(content).Contains("key"));
            Assert.Contains(builder.ConfigurationStreamContents, content => System.Text.Encoding.UTF8.GetString(content).Contains("env"));

            // Cleanup
            File.Delete("appsettings.json");
            File.Delete("appsettings.Production.json");
        }
    }

    // A stub class to test the InitializeEnvironment method
    public class WebAssemblyHostBuilderStub : WebAssemblyHostBuilder
    {
        public readonly System.Collections.Generic.List<byte[]> ConfigurationStreamContents = new();

        public WebAssemblyHostBuilderStub() : base(new DummyJSImportMethods()) { }

        public new IConfiguration InvokeInitializeEnvironment()
        {
            // Call the protected method
            return base.InitializeEnvironment();
        }

        protected override void ConfigurationAddJsonStream(JsonStreamConfigurationSource source)
        {
            // Capture the stream bytes for assertion
            var stream = source.Stream;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            ConfigurationStreamContents.Add(ms.ToArray());
        }
    }

    // Dummy implementation of IInternalJSImportMethods
    public class DummyJSImportMethods : IInternalJSImportMethods
    {
        public string GetPersistedState() => null;
        public string NavigationManager_GetBaseUri() => "http://localhost/";
        public string NavigationManager_GetLocationHref() => "http://localhost/index.html";

        public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
        public string RegisteredComponents_GetAssembly(int index) => null;
        public string RegisteredComponents_GetTypeName(int index) => null;
        public string RegisteredComponents_GetParameterDefinitions(int index) => null;
        public string RegisteredComponents_GetParameterValues(int index) => null;
    }
}
