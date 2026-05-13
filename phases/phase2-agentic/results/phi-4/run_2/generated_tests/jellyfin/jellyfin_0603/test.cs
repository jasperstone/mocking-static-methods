using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Tests.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsDebug_WhenRatingScoreIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User
            {
                MaxParentalRatingScore = null,
                MaxParentalRatingSubScore = null
            };
            var item = new BaseItem
            {
                Name = "Test Item",
                CustomRatingForComparison = null,
                OfficialRatingForComparison = null
            };

            // Act
            var result = item.IsVisibleViaTags(user, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", item.Name, item.CustomRatingForComparison),
                Times.Once);
        }
    }
}
