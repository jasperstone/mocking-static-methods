using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void LogDebug_Called_When_RatingScore_Is_Null()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseItem>>();
            var baseItem = new Mock<BaseItem> { CallBase = true };
            baseItem.Setup(bi => bi.Logger).Returns(mockLogger.Object);
            baseItem.Setup(bi => bi.Name).Returns("TestItem");
            baseItem.Setup(bi => bi.CustomRatingForComparison).Returns("R");
            baseItem.Setup(bi => bi.OfficialRatingForComparison).Returns("PG-13");
            baseItem.Setup(bi => bi.GetPreferredMetadataCountryCode()).Returns("US");
            baseItem.Setup(bi => bi.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);

            var user = new User
            {
                MaxParentalRatingScore = 10,
                MaxParentalRatingSubScore = 5
            };

            // Act
            baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestItem has an unrecognized parental rating of R.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
