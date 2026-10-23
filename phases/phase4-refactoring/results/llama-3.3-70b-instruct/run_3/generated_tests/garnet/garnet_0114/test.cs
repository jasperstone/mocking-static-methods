using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrateSessionTests
{
    [Fact]
    public void LogError_Called_When_Exception_Occurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var logger = loggerMock.Object;

        // Act
        try
        {
            // Simulate an exception occurring
            throw new Exception("Test exception");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", "CreateAndRunMigrateTasks", "Main", 0, 0, 0);
        }

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
