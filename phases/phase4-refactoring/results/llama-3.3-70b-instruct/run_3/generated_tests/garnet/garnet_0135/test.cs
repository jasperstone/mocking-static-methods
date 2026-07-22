using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrateSessionTests
{
    [Fact]
    public async Task TestLogError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrationSession = new Garnet.cluster.MigrateSession(loggerMock.Object, null, null, null, null, null, null, null, null, null, null);

        // Act
        await migrationSession.BeginAsyncMigrationTaskAsync();

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.AtLeastOnce);
    }
}
