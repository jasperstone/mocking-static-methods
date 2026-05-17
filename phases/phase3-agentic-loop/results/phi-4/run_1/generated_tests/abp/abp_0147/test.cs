using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Bundling;
using System.IO.Abstractions; // Assuming IFileSystem is used for file operations

public class BundlingServiceTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WhenGeneratingScriptReferences()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var fileSystemMock = new Mock<IFileSystem>();
        var directoryInfoMock = new Mock<IDirectoryInfo>();
        var directoryInfoEnumerableMock = new Mock<IEnumerable<IDirectoryInfo>>();

        directoryInfoMock.Setup(d => d.GetDirectories()).Returns(directoryInfoEnumerableMock.Object);
        directoryInfoEnumerableMock.Setup(e => e.GetEnumerator()).Returns(Enumerable.Empty<IDirectoryInfo>().GetEnumerator());

        fileSystemMock.Setup(f => f.Directory).Returns(new Mock<IDirectory>(directoryInfoMock.Object).Object);

        var bundlingService = new BundlingService
        {
            Logger = loggerMock.Object,
            FileSystem = fileSystemMock.Object // Assuming BundlingService has a FileSystem property
        };

        var bundleConfig = new BundleConfig
        {
            Mode = BundlingMode.None,
            InteractiveAuto = false
        };

        // Act
        await bundlingService.BundleAsync("testDirectory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Generating script references..."),
            Times.Once);
    }
}
