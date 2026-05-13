using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

public class BaseItemTests
{
    [Fact]
    public void IsVisibleViaTags_UnrecognizedParentalRating_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BaseItem>>();
        var baseItem = new Mock<BaseItem> { CallBase = true };
        baseItem.SetupGet(b => b.Name).Returns("Test Item");
        baseItem.SetupGet(b => b.CustomRatingForComparison).Returns("Unrecognized Rating");
        baseItem.SetupGet(b => b.OfficialRatingForComparison).Returns(string.Empty);
        var user = new User { MaxParentalRatingScore = 10, MaxParentalRatingSubScore = 5 };

        // Act
        var result = baseItem.Object.IsVisibleViaTags(user, false);

        // Assert
        loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
