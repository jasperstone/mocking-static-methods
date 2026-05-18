using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNull()
        {
            // Arrange
            var baseItem = new Mock<BaseItem>();
            var user = (User)null;

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNotVisible()
        {
            // Arrange
            var baseItem = new Mock<BaseItem>();
            var user = new User();

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnTrue_WhenUserIsVisible()
        {
            // Arrange
            var baseItem = new Mock<BaseItem>();
            var user = new User();

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void LogDebug_ShouldBeCalled_WhenRatingIsUnrecognized()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(x => x.IsVisibleViaTags(It.IsAny<User>(), It.IsAny<bool>())).Returns(true);
            baseItem.Setup(x => x.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);
            baseItem.Setup(x => x.CustomRatingForComparison).Returns("R");
            baseItem.Setup(x => x.OfficialRatingForComparison).Returns("PG");
            baseItem.Setup(x => x.GetPreferredMetadataCountryCode()).Returns("US");
            baseItem.Setup(x => x.Name).Returns("TestItem");
            baseItem.Setup(x => x.Logger).Returns(loggerMock.Object);

            var user = new User();

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestItem has an unrecognized parental rating of R.")),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
