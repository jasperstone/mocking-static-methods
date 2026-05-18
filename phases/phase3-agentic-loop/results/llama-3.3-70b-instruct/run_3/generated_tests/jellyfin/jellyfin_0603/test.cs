using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Name).Returns("Test Item");
            baseItem.SetupGet(b => b.CustomRatingForComparison).Returns("Unrecognized Rating");
            baseItem.SetupGet(b => b.OfficialRatingForComparison).Returns(string.Empty);
            var user = new MediaBrowser.Controller.Entities.User { MaxParentalRatingScore = null, MaxParentalRatingSubScore = null };

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
