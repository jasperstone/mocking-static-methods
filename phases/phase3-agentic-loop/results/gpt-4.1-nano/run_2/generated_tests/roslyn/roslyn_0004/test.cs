using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger> _loggerMock;

        public BuildHostProcessManagerTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger>();
            _loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        }

        [Fact]
        public async Task LogInformation_IsCalled_WhenDotnetPathIsNullAndProcessPathDiffers()
        {
            // Arrange
            var manager = new TestBuildHostProcessManager(_loggerFactoryMock.Object);
            var buildHostProcessMock = new Mock<BuildHostProcess>();
            buildHostProcessMock.Setup(p => p.BuildHost).ReturnsAsync(new FakeBuildHost());
            var buildHostProcess = buildHostProcessMock.Object;

            // Setup process info
            var processPath = "C:\\Program Files\\dotnet\\dotnet.exe";
            var msbuildLocationPath = "C:\\SDK\\sdk\\somepath";

            // Mock static method GetProcessPath
            var getProcessPathCalled = false;
            var originalGetProcessPath = BuildHostProcessManager.GetProcessPath;
            BuildHostProcessManager.GetProcessPath = () => processPath;
            try
            {
                // Setup FindBestMSBuildAsync to return a location
                var msbuildLocation = new FakeMSBuildLocation(msbuildLocationPath);
                var buildHostMock = new Mock<RemoteBuildHost>();
                buildHostMock.Setup(h => h.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(msbuildLocation);

                // Act
                var result = await manager.GetBuildHostAsync(
                    BuildHostProcessKind.NetCore,
                    "project.csproj",
                    null,
                    CancellationToken.None);

                // Assert
                _loggerMock.Verify(
                    log => log.LogInformation(
                        It.Is<string>(s => s.Contains("reloading to start from")),
                        processPath,
                        It.Is<string>(p => p == msbuildLocationPath + "/../../dotnet.exe")),
                    Times.Once);
            }
            finally
            {
                // Restore static method
                BuildHostProcessManager.GetProcessPath = originalGetProcessPath;
            }
        }
    }

    // Helper classes for mocking
    internal class FakeBuildHost : RemoteBuildHost
    {
        public override Task<bool> HasUsableMSBuildAsync(string projectFilePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    internal class FakeMSBuildLocation
    {
        public string Path { get; }

        public FakeMSBuildLocation(string path)
        {
            Path = path;
        }
    }

    // Extending the class to allow static method override for testing
    internal class TestBuildHostProcessManager : BuildHostProcessManager
    {
        public TestBuildHostProcessManager(ILoggerFactory loggerFactory)
            : base(null, null, loggerFactory)
        {
        }

        public new static Func<string> GetProcessPath = () =>
        {
            throw new NotImplementedException();
        };

        public new static string GetProcessPathMethod()
        {
            return GetProcessPath();
        }
    }
}
