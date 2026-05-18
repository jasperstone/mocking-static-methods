using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Bit.Migrator;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_ShouldCallLogInformationOnLogger()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = mockLogger.Object;
        var dbUpLogger = new DbUpLogger(logger);
        var eventId = 123; // Placeholder for Constants.BypassFiltersEventId
        var message = "Test message";
        var formattedMessage = string.Format(message, new object[] { });

        // Act
        dbUpLogger.LogInformation(message);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.Is<EventId>(id => id.Id == eventId),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == formattedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
