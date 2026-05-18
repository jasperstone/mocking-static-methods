using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebAssembly.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsConfigurationStream_WhenFileExists()
        {
            // Arrange
            var jsMethods = new DummyJsMethods();
            var builder = new WebAssemblyHostBuilder(jsMethods);

            var testJsonContent = "{ \"TestKey\": \"TestValue\" }";
            var fileName = "appsettings.json";
            File.WriteAllText(fileName, testJsonContent);

            // Use reflection to invoke the private method
            var methodInfo = typeof(WebAssemblyHostBuilder).GetMethod("InitializeEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            var result = methodInfo.Invoke(builder, null);

            // Assert
            var config = builder.Configuration as WebAssemblyHostConfiguration;
            Assert.NotNull(config);
            Assert.Equal("TestValue", config["TestKey"]);

            // Cleanup
            File.Delete(fileName);
        }

        [Fact]
        public void InitializeEnvironment_DoesNotAddConfigurationStream_WhenFileDoesNotExist()
        {
            // Arrange
            var jsMethods = new DummyJsMethods();
            var builder = new WebAssemblyHostBuilder(jsMethods);

            // Ensure the file does not exist
            var fileName = "appsettings.json";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            // Use reflection to invoke the private method
            var methodInfo = typeof(WebAssemblyHostBuilder).GetMethod("InitializeEnvironment", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            var result = methodInfo.Invoke(builder, null);

            // Assert
            var config = builder.Configuration as WebAssemblyHostConfiguration;
            Assert.NotNull(config);
            // Since no file, the configuration should be empty
            Assert.Null(config["TestKey"]);
        }
    }

    // Dummy implementation for testing
    public class DummyJsMethods : IInternalJSImportMethods
    {
        public string NavigationManager_GetBaseUri() => "http://localhost/";
        public string NavigationManager_GetLocationHref() => "http://localhost/index.html";
        public string GetApplicationEnvironment() => "Development";
        public int RegisterComponentCount => 0;
        public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
        public string RegisteredComponents_GetAssembly(int index) => null;
        public string RegisteredComponents_GetTypeName(int index) => null;
        public string RegisteredComponents_GetParameterDefinitions(int index) => null;
        public string RegisteredComponents_GetParameterValues(int index) => null;
        public string GetPersistedState() => null;
    }
}
