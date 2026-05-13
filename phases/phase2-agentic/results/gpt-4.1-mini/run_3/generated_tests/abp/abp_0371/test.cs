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

            // We create a minimal subclass to override dependencies not needed for this test
            _packageAdder = new TestProjectNpmPackageAdder(_cmdHelperMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInformation_WhenPackageJsonNotFound()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            // Ensure directory exists but no package.json
            Directory.CreateDirectory(directory);

            await _packageAdder.AddNpmPackageAsync(directory, npmPackage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("package.json not found!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Directory.Delete(directory);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsInstallAndRunsCmd_WhenPackageNotInPackageJson()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            // Write package.json without the package name
            await File.WriteAllTextAsync(packageJsonPath, "{}");

            await _packageAdder.AddNpmPackageAsync(directory, npmPackage, version: "1.2.3");

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Installing '{npmPackage.Name}' package")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("yarn add test-package@1.2.3")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd("npx yarn add test-package@1.2.3"), Times.Once);

            File.Delete(packageJsonPath);
            Directory.Delete(directory);
        }

        [Fact]
        public async Task AddNpmPackageAsync_LogsAlreadyInstalled_WhenPackageInPackageJson()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);
            var packageJsonPath = Path.Combine(directory, "package.json");
            var npmPackage = new NpmPackageInfo { Name = "test-package" };

            // Write package.json containing the package name
            await File.WriteAllTextAsync(packageJsonPath, $"\"{npmPackage.Name}\"");

            await _packageAdder.AddNpmPackageAsync(directory, npmPackage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"'{npmPackage.Name}' is already installed.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);

            File.Delete(packageJsonPath);
            Directory.Delete(directory);
        }

        private class TestProjectNpmPackageAdder : ProjectNpmPackageAdder
        {
            private readonly ICmdHelper _cmdHelper;

            public TestProjectNpmPackageAdder(ICmdHelper cmdHelper) : base(
                jsonSerializer: null,
                sourceCodeDownloadService: null,
                angularSourceCodeAdder: null,
                remoteServiceExceptionHandler: null,
                installLibsService: null,
                cmdHelper: cmdHelper,
                cliHttpClientFactory: null,
                npmPackageInfoProvider: null)
            {
                _cmdHelper = cmdHelper;
            }

            protected override async Task<bool> DownloadAngularSourceCode(string angularDirectory, NpmPackageInfo package, string version = null)
            {
                // Skip actual download for tests
                return await Task.FromResult(false);
            }
        }
    }

    // Minimal stub for NpmPackageInfo to allow compilation
    public class NpmPackageInfo
    {
        public string Name { get; set; }
    }

    // Minimal stub for ICmdHelper to allow compilation
    public interface ICmdHelper
    {
        void RunCmd(string command);
    }
}
