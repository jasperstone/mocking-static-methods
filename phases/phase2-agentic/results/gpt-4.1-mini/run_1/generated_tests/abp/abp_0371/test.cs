using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.ProjectModification;
using Xunit;

namespace Volo.Abp.Cli.Tests.ProjectModification
{
    public class ProjectNpmPackageAdderTests
    {
        private readonly Mock<ILogger<ProjectNpmPackageAdder>> _loggerMock;
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly ProjectNpmPackageAdder _packageAdder;

        public ProjectNpmPackageAdderTests()
        {
            _loggerMock = new Mock<ILogger<ProjectNpmPackageAdder>>();
            _cmdHelperMock = new Mock<ICmdHelper>();

            // We only need to test AddNpmPackageAsync method and the logging on line 83.
            // So we create a minimal subclass to override dependencies not used in this test.
            _packageAdder = new TestProjectNpmPackageAdder(_cmdHelperMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExists_AndPackageNotInstalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var packageJsonPath = Path.Combine(tempDir, "package.json");
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };
            var version = "1.2.3";

            // Write package.json without the package name to simulate package not installed
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage, version);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"yarn add {npmPackageName}@{version}"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd($"npx yarn add {npmPackageName}@{version}"), Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonExists_AndPackageAlreadyInstalled()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var packageJsonPath = Path.Combine(tempDir, "package.json");
            var npmPackageName = "test-package";
            var npmPackage = new NpmPackageInfo { Name = npmPackageName };

            // Write package.json containing the package name to simulate package already installed
            await File.WriteAllTextAsync(packageJsonPath, $"\"{npmPackageName}\"");

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackageName}' package")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"'{npmPackageName}' is already installed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsError_WhenPackageJsonDoesNotExist()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            // Act
            await _packageAdder.AddNpmPackageAsync(tempDir, npmPackage);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "package.json not found!"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        private class TestProjectNpmPackageAdder : ProjectNpmPackageAdder
        {
            private readonly ICmdHelper _cmdHelper;

            public TestProjectNpmPackageAdder(ICmdHelper cmdHelper)
                : base(
                    jsonSerializer: null!,
                    sourceCodeDownloadService: null!,
                    angularSourceCodeAdder: null!,
                    remoteServiceExceptionHandler: null!,
                    installLibsService: null!,
                    cmdHelper: cmdHelper,
                    cliHttpClientFactory: null!,
                    npmPackageInfoProvider: null!)
            {
                _cmdHelper = cmdHelper;
            }

            public override ILogger<ProjectNpmPackageAdder> Logger { get; set; } = null!;

            protected override async Task<bool> DownloadAngularSourceCode(string angularDirectory, NpmPackageInfo package, string version = null)
            {
                // Skip actual download in tests
                return await Task.FromResult(false);
            }
        }

        private class NpmPackageInfo
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
