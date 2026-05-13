using Xunit;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_ShouldAddConfiguration()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost");

            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);

            var configFileContent = "{\"Key\": \"Value\"}";
            var configFilePath = "appsettings.json";
            File.WriteAllText(configFilePath, configFileContent);

            // Act
            var environment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(environment);
            Assert.Equal("Development", environment.Environment);
            Assert.Equal("http://localhost", environment.BaseAddress);

            var config = builder.Configuration;
            Assert.NotNull(config);
            Assert.Equal("Value", config["Key"]);

            // Clean up
            File.Delete(configFilePath);
        }
    }
}
