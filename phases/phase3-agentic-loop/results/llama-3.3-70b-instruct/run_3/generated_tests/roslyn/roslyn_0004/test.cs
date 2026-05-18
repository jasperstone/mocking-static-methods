using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Reloading_BuildHost()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerFactoryMock.Object);

            // Act
            await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "projectFilePath", "dotnetPath", CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
