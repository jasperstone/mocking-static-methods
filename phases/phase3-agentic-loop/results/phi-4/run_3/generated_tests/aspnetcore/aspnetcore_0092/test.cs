using System.IO;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Microsoft.Extensions.Configuration.Tests
{
    public class WebAssemblyHostBuilderTests
    {
        [Fact]
        public void InitializeEnvironment_AddsJsonStreamConfigurationSource()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");

            var configurationBuilderMock = new Mock<IConfigurationBuilder>();
            var webAssemblyHostBuilder = new WebAssemblyHostBuilder(jsMethodsMock.Object)
            {
                Configuration = configurationBuilderMock.Object
            };

            // Simulate the existence of appsettings.json
            jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost");
            jsMethodsMock.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/index.html");

            // Act
            webAssemblyHostBuilder.InitializeEnvironment();

            // Assert
            configurationBuilderMock.Verify(
                cb => cb.Add(It.Is<JsonStreamConfigurationSource>(source =>
                    source.Stream != null &&
                    source.Stream.Length > 0)), // Ensure the stream is set and not empty
                Times.Exactly(1)); // Assuming one config file is found and added
        }
    }
}
