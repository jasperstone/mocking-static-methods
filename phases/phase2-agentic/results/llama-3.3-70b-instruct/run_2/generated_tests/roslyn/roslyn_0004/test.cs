using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogInformation_Called_When_Reloading_BuildHost()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            await buildHostProcessManager.GetBuildHostAsync(BuildHostProcessKind.NetCore, "projectFilePath", "dotnetPath", CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
