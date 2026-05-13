using System.IO;
using System.IO.Pipes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Xunit;

public class WebAssemblyHostBuilderTests
{
    [Fact]
    public void InitializeEnvironment_AddsJsonStreamConfigurationSource()
    {
        // Arrange
        var jsMethodsMock = new Mock<IInternalJSImportMethods>();
        jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");

        var configurationBuilderMock = new Mock<IConfigurationBuilder>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.Build()).Returns(configurationMock.Object);

        configurationBuilderMock.Setup(b => b.Add<JsonStreamConfigurationSource>(It.IsAny<Action<JsonStreamConfigurationSource>>()))
            .Callback<Action<JsonStreamConfigurationSource>>(action =>
            {
                var source = new JsonStreamConfigurationSource();
                action(source);
                Assert.NotNull(source.Stream);
            });

        jsMethodsMock.Setup(m => m.NavigationManager_GetBaseUri()).Returns("http://localhost");
        jsMethodsMock.Setup(m => m.NavigationManager_GetLocationHref()).Returns("http://localhost/index.html");

        // Simulate file existence
        jsMethodsMock.Setup(m => m.File_Exists(It.IsAny<string>())).Returns(true);
        jsMethodsMock.Setup(m => m.File_ReadAllBytes(It.IsAny<string>())).Returns(new byte[] { /* JSON content */ });

        var hostBuilder = new WebAssemblyHostBuilder(jsMethodsMock.Object)
        {
            Configuration = configurationBuilderMock.Object
        };

        // Act
        hostBuilder.InitializeEnvironment();

        // Assert
        configurationBuilderMock.Verify(b => b.Add<JsonStreamConfigurationSource>(It.IsAny<Action<JsonStreamConfigurationSource>>()), Times.Once);
    }
}
