using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformation_WhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            internal class TestReseedFolderFlag : ReseedFolderFlag
            {
                public TestReseedFolderFlag(
                        IStartupLogger<MigrateLibraryDb> startupLogger,
                        IDbContextFactory<JellyfinDbContext> provider,
                        IServerApplicationPaths paths)
                    : base(startupLogger, provider, paths)
                {
                }

                public async Task PerformAsync(CancellationToken cancellationToken)
                {
                    await base.PerformAsync(cancellationToken);
                }
            }

            var reseedFolderFlag = new TestReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
