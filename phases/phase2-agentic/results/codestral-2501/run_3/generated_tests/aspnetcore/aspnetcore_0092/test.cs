using Xunit;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsConfigurationFromAppSettingsJson()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");

            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);

            var appSettingsJson = "{\"Key\": \"Value\"}";
            var appSettingsBytes = Encoding.UTF8.GetBytes(appSettingsJson);
            File.WriteAllBytes("appsettings.json", appSettingsBytes);

            // Act
            var environment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(environment);
            Assert.Equal("Development", environment.Environment);
            Assert.Equal("http://localhost/", environment.BaseAddress);

            var configuration = builder.Configuration;
            Assert.NotNull(configuration);
            Assert.Equal("Value", configuration["Key"]);

            // Clean up
            File.Delete("appsettings.json");
        }
    }
}
