using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var user = new MediaBrowser.Controller.Entities.User();
            var baseItem = new MediaBrowser.Controller.Entities.Movie
            {
                Name = "Test Item",
                CustomRatingForComparison = "Unrecognized Rating",
                Logger = loggerMock.Object
            };

            // Act
            baseItem.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
