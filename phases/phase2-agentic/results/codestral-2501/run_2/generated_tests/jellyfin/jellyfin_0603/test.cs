using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_ShouldLogDebug_WhenRatingIsUnrecognized()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseItem>>();
            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(x => x.Name).Returns("TestItem");
            baseItem.Setup(x => x.CustomRatingForComparison).Returns("UnrecognizedRating");
            baseItem.Setup(x => x.OfficialRatingForComparison).Returns("UnrecognizedRating");
            baseItem.Setup(x => x.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);
            baseItem.Setup(x => x.GetPreferredMetadataCountryCode()).Returns("US");
            baseItem.Setup(x => x.Logger).Returns(mockLogger.Object);

            var user = new User
            {
                MaxParentalRatingScore = 10,
                MaxParentalRatingSubScore = 5
            };

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", "TestItem", "UnrecognizedRating"),
                Times.Once);
        }
    }
}
