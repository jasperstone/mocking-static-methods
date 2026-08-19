using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var user = new User
            {
                MaxParentalRatingScore = 10,
                MaxParentalRatingSubScore = 5
            };

            var baseItem = new Movie
            {
                Name = "Test Item",
                CustomRatingForComparison = "Unrecognized Rating"
            };

            // Act
            baseItem.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
