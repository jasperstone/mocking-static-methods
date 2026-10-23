using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Entities
{
    public class MovieTests
    {
        [Fact]
        public void IsVisibleToUser_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Movie>>();
            var userMock = new Mock<User>();
            userMock.Setup(u => u.MaxParentalRatingScore).Returns((int?)10);
            userMock.Setup(u => u.MaxParentalRatingSubScore).Returns((int?)5);
            var movie = new Movie();
            movie.Logger = loggerMock.Object;
            movie.Name = "Test Item";
            movie.CustomRatingForComparison = "Unrecognized Rating";

            // Act
            movie.IsVisibleToUser(userMock.Object, false);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
