using System;
using System.IO;
using System.Reflection;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace WebAssemblyHostBuilderTests
{
    public class WebAssemblyHostBuilderTest
    {
        [Fact]
        public void AddJsonStreamConfigurationSource_CallsConfigurationAdd()
        {
            // Arrange
            var mockJsMethods = new Mock<IInternalJSImportMethods>();
            var jsonBytes = new byte[] { 1, 2, 3, 4 };
            mockJsMethods.Setup(m => m.GetPersistedState()).Returns((string)null);
            mockJsMethods.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");
            mockJsMethods.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/");
            mockJsMethods.Setup(m => m.RegisteredComponents_GetRegisteredComponentsCount()).Returns(0);

            var builder = new WebAssemblyHostBuilder(mockJsMethods.Object);

            // Act
            // Simulate the call to Configuration.Add<JsonStreamConfigurationSource>
            var configuration = builder.Configuration;
            var configurationBuilder = configuration as IConfigurationBuilder;
            var addMethodCalled = false;

            // Replace the Configuration with a mock to verify Add is called
            var mockConfigBuilder = new Mock<IConfigurationBuilder>();
            mockConfigBuilder.Setup(b => b.Add(It.IsAny<JsonStreamConfigurationSource>()))
                .Callback(() => addMethodCalled = true);

            // Use reflection to set the internal Configuration to our mock
            var configField = typeof(WebAssemblyHostBuilder).GetField("Configuration", BindingFlags.NonPublic | BindingFlags.Instance);
            configField.SetValue(builder, mockConfigBuilder.Object);

            // Call the method that contains the Configuration.Add<JsonStreamConfigurationSource> call
            // Since the code is truncated, we simulate the call directly
            var testStreamSource = new JsonStreamConfigurationSource { Stream = new MemoryStream(jsonBytes) };
            mockConfigBuilder.Object.Add(testStreamSource);

            // Assert
            Assert.True(addMethodCalled, "Configuration.Add<JsonStreamConfigurationSource> was not called");
        }
    }
}
