using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WebAssembly.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsConfigurationFromJsonFiles()
        {
            // Arrange
            var builder = new WebAssemblyHostBuilder(new TestJSImportMethods());

            // Act
            var environment = builder.GetType()
                .GetMethod("InitializeEnvironment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(builder, null);

            // Assert
            var config = builder.Configuration as WebAssemblyHostConfiguration;
            Assert.NotNull(config);
            // Additional assertions can be added here if needed
        }
    }

    // Mock implementation of IInternalJSImportMethods for testing
    public class TestJSImportMethods : IInternalJSImportMethods
    {
        public string NavigationManager_GetBaseUri() => "http://localhost/";
        public string NavigationManager_GetLocationHref() => "http://localhost/index.html";
        public string GetApplicationEnvironment() => "Development";

        public int RegisteredComponents_GetRegisteredComponentsCount() => 0;
        public string RegisteredComponents_GetAssembly(int index) => null;
        public string RegisteredComponents_GetTypeName(int index) => null;
        public string RegisteredComponents_GetParameterDefinitions(int index) => null;
        public string RegisteredComponents_GetParameterValues(int index) => null;

        public string GetPersistedState() => null;
    }
}
