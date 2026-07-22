using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
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
        public void LogDebug_ShouldBeCalled_WhenRatingIsUnrecognized()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var baseItem = new Mock<BaseItem>().Object;
            baseItem.Logger = loggerMock.Object;

            // Mock User
            var userMock = new Mock<User>();
            userMock.Setup(u => u.MaxParentalRatingScore).Returns((int?)null);
            userMock.Setup(u => u.MaxParentalRatingSubScore).Returns((int?)null);

            // Act
            baseItem.IsVisibleViaTags(userMock.Object, false);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
