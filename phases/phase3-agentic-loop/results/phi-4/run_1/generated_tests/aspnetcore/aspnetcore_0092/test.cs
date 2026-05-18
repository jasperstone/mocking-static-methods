using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenAppSettingsFileExists()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");

            var webAssemblyHostBuilder = new WebAssemblyHostBuilder(jsMethodsMock.Object);

            // Simulate the existence of "appsettings.json" and its content
            var appSettingsContent = "{\"Key\":\"Value\"}";
            var appSettingsStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(appSettingsContent));

            // Mock File.Exists to return true for "appsettings.json"
            webAssemblyHostBuilder.Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "appsettings.json", appSettingsContent }
                })
                .Build();

            // Act
            var hostEnvironment = webAssemblyHostBuilder.InitializeEnvironment();

            // Assert
            Assert.NotNull(hostEnvironment);
            Assert.Contains("Key", webAssemblyHostBuilder.Configuration.AsEnumerable());
            Assert.Equal("Value", webAssemblyHostBuilder.Configuration["Key"]);
        }
    }
}
