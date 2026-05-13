using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Moq;
using Xunit;

public class WebAssemblyHostBuilderTests
{
    [Fact]
    public void InitializeEnvironment_AddsJsonStreamConfigurationSource_WhenConfigFilesExist()
    {
        // Arrange
        var jsMethodsMock = new Mock<IInternalJSImportMethods>();
        jsMethodsMock.Setup(m => m.GetApplicationEnvironment()).Returns("Development");

        var configurationBuilderMock = new Mock<IConfigurationBuilder>();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.Build()).Returns(configurationMock.Object);

        configurationBuilderMock.Setup(b => b.Build()).Returns(configurationMock.Object);

        var fileSystemMock = new Mock<IFileSystem>();
        fileSystemMock.Setup(f => f.File.Exists(It.IsAny<string>())).Returns(true);
        fileSystemMock.Setup(f => f.File.ReadAllBytes(It.IsAny<string>())).Returns(new byte[] { 0x7b, 0x22, 0x61, 0x70, 0x70, 0x73, 0x65, 0x74, 0x74, 0x69, 0x6e, 0x67, 0x73, 0x22, 0x3a, 0x7b });

        var webAssemblyHostBuilder = new WebAssemblyHostBuilder(jsMethodsMock.Object)
        {
            Configuration = configurationBuilderMock.Object,
            FileSystem = fileSystemMock.Object
        };

        // Act
        webAssemblyHostBuilder.InitializeEnvironment();

        // Assert
        configurationBuilderMock.Verify(
            b => b.Add<JsonStreamConfigurationSource>(
                It.Is<JsonStreamConfigurationSource>(s => s.Stream != null && s.Stream.Length > 0)),
            Times.Once);
    }
}
