using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void LogDebug_ShouldBeCalled_WhenRatingIsUnrecognized()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItemMock = new Mock<BaseItem>();
            baseItemMock.Setup(b => b.Name).Returns("TestItem");
            baseItemMock.Setup(b => b.CustomRatingForComparison).Returns("UnrecognizedRating");
            baseItemMock.Setup(b => b.OfficialRatingForComparison).Returns("UnrecognizedRating");
            baseItemMock.Setup(b => b.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);
            baseItemMock.Setup(b => b.Logger).Returns(loggerMock.Object);

            var user = new User();

            // Act
            var result = baseItemMock.Object.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug(
                    "{0} has an unrecognized parental rating of {1}.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
