using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Users;
using MediaBrowser.Model.Entities; // Assuming ParentalRatingScore is here
using MediaBrowser.Controller.Localization; // Assuming ILocalizationManager is here

namespace MediaBrowser.Tests.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void IsVisibleViaTags_LogsDebug_WhenRatingIsUnrecognized()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User
            {
                MaxParentalRatingScore = null,
                MaxParentalRatingSubScore = null
            };

            var localizationManagerMock = new Mock<ILocalizationManager>();
            localizationManagerMock.Setup(lm => lm.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                                   .Returns((ParentalRatingScore?)null);

            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(b => b.Name).Returns("Test Item");
            baseItem.Setup(b => b.CustomRatingForComparison).Returns(string.Empty);
            baseItem.Setup(b => b.OfficialRatingForComparison).Returns(string.Empty);
            baseItem.Setup(b => b.GetBlockUnratedValue(It.IsAny<User>())).Returns(false);
            baseItem.Setup(b => b.LocalizationManager).Returns(localizationManagerMock.Object);

            // Act
            var result = baseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            loggerMock.Verify(l => l.LogDebug("{0} has an unrecognized parental rating of {1}.", "Test Item", string.Empty), Times.Once);
            Assert.False(result);
        }
    }
}
