using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogDebug_Called_WithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var itemName = "TestItem";
            var rating = "UnrecognizedRating";

            // Act
            loggerMock.Object.LogDebug("{0} has an unrecognized parental rating of {1}.", itemName, rating);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"{itemName} has an unrecognized parental rating of {rating}.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
