using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleToUser_UnrecognizedParentalRating_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItem = new BaseItem();
            baseItem.Logger = loggerMock.Object;
            baseItem.Name = "Test Item";
            baseItem.CustomRatingForComparison = "Unrecognized Rating";
            var user = new User { MaxParentalRatingScore = 10, MaxParentalRatingSubScore = 10 };

            // Act
            baseItem.IsVisibleToUser(user, false);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
