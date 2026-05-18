using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task GetBuildHostWithFallbackAsync_LogsWarning_WhenMonoMSBuildNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var binLogPathProviderMock = new Mock<IBinLogPathProvider>();

            var manager = new BuildHostProcessManager(
                globalMSBuildProperties: null,
                binaryLogPathProvider: binLogPathProviderMock.Object,
                loggerFactory: loggerFactoryMock.Object);

            // Act
            await manager.GetBuildHostWithFallbackAsync("testProject.csproj", CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("An installation of Mono MSBuild could not be found"))),
                Times.Once);
        }
    }
}
