using Xunit;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;
using System.Text;

namespace WebAssemblyHostBuilderTests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_ShouldAddJsonConfiguration()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");
            jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost/");

            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);
            var configuration = builder.Configuration;

            var appSettingsJson = "{\"Key\": \"Value\"}";
            var appSettingsBytes = Encoding.UTF8.GetBytes(appSettingsJson);
            File.WriteAllBytes("appsettings.json", appSettingsBytes);
            File.WriteAllBytes("appsettings.Development.json", appSettingsBytes);

            // Act
            var environment = builder.InitializeEnvironment();

            // Assert
            Assert.NotNull(environment);
            Assert.Contains(configuration, c => c is JsonStreamConfigurationSource);
        }
    }
}
