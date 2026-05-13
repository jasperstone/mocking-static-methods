using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNull()
        {
            // Arrange
            var baseItem = new Mock<BaseItem>().Object;

            // Act
            var result = baseItem.IsVisibleViaTags(null, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNotVisible()
        {
            // Arrange
            var user = new UserData { MaxParentalRatingScore = 10, MaxParentalRatingSubScore = 5 };
            var baseItem = new Mock<BaseItem>().Object;
            var mockLogger = new Mock<ILogger<BaseItem>>();
            baseItem.Logger = mockLogger.Object;

            // Act
            var result = baseItem.IsVisibleViaTags(user, false);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnTrue_WhenUserIsVisible()
        {
            // Arrange
            var user = new UserData { MaxParentalRatingScore = 10, MaxParentalRatingSubScore = 5 };
            var baseItem = new Mock<BaseItem>().Object;
            var mockLogger = new Mock<ILogger<BaseItem>>();
            baseItem.Logger = mockLogger.Object;

            // Act
            var result = baseItem.IsVisibleViaTags(user, true);

            // Assert
            Assert.True(result);
            mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
