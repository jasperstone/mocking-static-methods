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
        public async Task LogInformation_IsCalled_WhenConditionsAreMet()
        {
            // Arrange
            var manager = new TestBuildHostProcessManager(_loggerFactoryMock.Object);
            var buildHostProcessMock = new Mock<BuildHostProcess>();
            var buildHostMock = new Mock<BuildHost>();
            buildHostMock.Setup(b => b.FindBestMSBuildAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MSBuildLocation("somePath"));
            buildHostProcessMock.Setup(p => p.BuildHost).Returns(buildHostMock.Object);
            buildHostProcessMock.Setup(p => p.Disconnected += It.IsAny<EventHandler>()).Verifiable();

            // Act
            await manager.TestGetBuildHostAsync(BuildHostProcessKind.NetCore, "someFile.csproj", null, CancellationToken.None, buildHostProcessMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains(".NET BuildHost started from")),
                    It.IsAny<object>(), It.IsAny<object>()),
                Times.Once);
        }
    }

    // Helper class to expose internal method for testing
    internal class TestBuildHostProcessManager : BuildHostProcessManager
    {
        public TestBuildHostProcessManager(ILoggerFactory loggerFactory)
            : base(globalMSBuildProperties: null, binaryLogPathProvider: null, loggerFactory: loggerFactory)
        {
        }

        public async Task<BuildHostProcess> TestGetBuildHostAsync(
            BuildHostProcessKind buildHostKind,
            string projectOrSolutionFilePath,
            string? dotnetPath,
            CancellationToken cancellationToken,
            BuildHostProcess mockBuildHostProcess)
        {
            // Call the internal method with a mock buildHostProcess
            return await base.GetBuildHostAsync(buildHostKind, projectOrSolutionFilePath, dotnetPath, cancellationToken);
        }
    }
}
