using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectModification;

public class ProjectNpmPackageAdderTests
{
    private class TestProjectNpmPackageAdder : ProjectNpmPackageAdder
    {
        public TestProjectNpmPackageAdder(
            ICmdHelper cmdHelper)
            : base(
                jsonSerializer: null,
                sourceCodeDownloadService: null,
                angularSourceCodeAdder: null,
                remoteServiceExceptionHandler: null,
                installLibsService: null,
                cmdHelper: cmdHelper,
                cliHttpClientFactory: null,
                npmPackageInfoProvider: null)
        {
            Logger = NullLogger<ProjectNpmPackageAdder>.Instance;
        }
    }

    [Fact]
    public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExistsAndPackageNotInstalled()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        var packageJsonPath = Path.Combine(directory, "package.json");
        var npmPackageName = "test-package";
        var npmPackage = new NpmPackageInfo { Name = npmPackageName };

        // Write package.json without the package name
        await File.WriteAllTextAsync(packageJsonPath, "{ \"dependencies\": {} }");

        var loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>()));

        var adder = new TestProjectNpmPackageAdder(cmdHelperMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await adder.AddNpmPackageAsync(directory, npmPackage);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add " + npmPackageName)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        cmdHelperMock.Verify(x => x.RunCmd(It.Is<string>(s => s.Contains("npx yarn add " + npmPackageName))), Times.Once);

        // Cleanup
        Directory.Delete(directory, true);
    }
}
