using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsUnrecognizedParentalRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Name).Returns("Test Item");
            baseItem.SetupGet(b => b.CustomRatingForComparison).Returns("Unrecognized Rating");
            baseItem.SetupGet(b => b.OfficialRatingForComparison).Returns(string.Empty);
            var user = new Mock<User>();
            user.SetupGet(u => u.MaxParentalRatingScore).Returns(10);
            user.SetupGet(u => u.MaxParentalRatingSubScore).Returns(5);

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user.Object, false);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
