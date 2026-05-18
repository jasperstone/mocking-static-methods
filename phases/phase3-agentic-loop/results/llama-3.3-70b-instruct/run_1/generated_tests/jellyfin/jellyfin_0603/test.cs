using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleToUser_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var baseItem = new Folder
            {
                Name = "Test Item"
            };
            var user = new MediaBrowser.Controller.Users.User { MaxParentalRatingScore = 10 };

            // Act
            var result = baseItem.IsVisibleToUser(user, false);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
