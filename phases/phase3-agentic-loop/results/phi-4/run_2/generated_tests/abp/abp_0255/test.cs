using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.ProjectBuilding.Building;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_LogsSuccessMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SourceCodeDownloadService>>();
        var service = new SourceCodeDownloadService(
            Mock.Of<ModuleProjectBuilder>(),
            Mock.Of<NugetPackageProjectBuilder>(),
            Mock.Of<NpmPackageProjectBuilder>()
        )
        {
            Logger = loggerMock.Object
        };

        var moduleProjectBuilderMock = Mock.Get(service.ModuleProjectBuilder);
        moduleProjectBuilderMock
            .Setup(m => m.BuildAsync(It.IsAny<ProjectBuildArgs>()))
            .ReturnsAsync(new ModuleProjectBuilder.BuildResult
            {
                ZipContent = new byte[0]
            });

        // Act
        await service.DownloadModuleAsync("TestModule", "outputFolder", "1.0.0", null, null, null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation($"'{It.Is<string>(s => s.Contains("TestModule"))}' has been successfully downloaded to 'outputFolder'"),
            Times.Once);
    }
}
