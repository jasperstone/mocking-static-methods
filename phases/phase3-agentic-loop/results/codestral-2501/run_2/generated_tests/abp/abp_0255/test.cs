using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Args;
using Xunit;

public class SourceCodeDownloadServiceTests
{
    [Fact]
    public async Task DownloadModuleAsync_ShouldLogSuccessMessage()
    {
        // Arrange
        var moduleProjectBuilder = Substitute.For<ModuleProjectBuilder>();
        var nugetPackageProjectBuilder = Substitute.For<NugetPackageProjectBuilder>();
        var npmPackageProjectBuilder = Substitute.For<NpmPackageProjectBuilder>();
        var logger = Substitute.For<ILogger<SourceCodeDownloadService>>();

        var service = new SourceCodeDownloadService(
            moduleProjectBuilder,
            nugetPackageProjectBuilder,
            npmPackageProjectBuilder
        )
        {
            Logger = logger
        };

        var moduleName = "TestModule";
        var outputFolder = Path.Combine(Path.GetTempPath(), "TestOutput");
        var version = "1.0.0";
        var gitHubAbpLocalRepositoryPath = "path/to/abp";
        var gitHubVoloLocalRepositoryPath = "path/to/volo";
        var options = new AbpCommandLineOptions();

        var projectBuildResult = new ProjectBuildResult(new byte[0], moduleName);

        moduleProjectBuilder
            .BuildAsync(Arg.Any<ProjectBuildArgs>())
            .Returns(projectBuildResult);

        // Act
        await service.DownloadModuleAsync(moduleName, outputFolder, version, gitHubAbpLocalRepositoryPath, gitHubVoloLocalRepositoryPath, options);

        // Assert
        logger.Received(1).LogInformation(
            Arg.Is<string>(s => s.Contains($"'{moduleName}' has been successfully downloaded to '{outputFolder}'")),
            Arg.Any<object[]>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }
}
