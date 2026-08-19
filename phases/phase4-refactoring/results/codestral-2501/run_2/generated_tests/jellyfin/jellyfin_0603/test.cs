using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void LogDebug_Called_WhenRatingIsUnrecognized()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseItem>>();
            var mockUser = new Mock<User>();
            var baseItem = new Mock<BaseItem> { CallBase = true };
            baseItem.Setup(x => x.Name).Returns("TestItem");
            baseItem.Setup(x => x.CustomRatingForComparison).Returns("UnrecognizedRating");
            baseItem.Setup(x => x.OfficialRatingForComparison).Returns("UnrecognizedRating");
            baseItem.Setup(x => x.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);
            baseItem.Setup(x => x.Logger).Returns(mockLogger.Object);

            // Act
            var result = baseItem.Object.IsVisibleViaTags(mockUser.Object, false);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", "TestItem", "UnrecognizedRating"),
                Times.Once);
        }
    }
}
