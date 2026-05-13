using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemParentalRatingTests
    {
        private sealed class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILogger logger, ILocalizationManager localizationManager, string name, string customRating, bool blockUnrated)
            {
                Logger = logger;
                LocalizationManager = localizationManager;
                Name = name;
                CustomRating = customRating;
                BlockUnrated = blockUnrated;
            }

            private string CustomRating { get; }

            private bool BlockUnrated { get; }

            protected override string CustomRatingForComparison => CustomRating;

            protected override bool GetBlockUnratedValue(User user) => BlockUnrated;

            protected override ILocalizationManager LocalizationManager { get; }

            protected override string OfficialRatingForComparison => null;
        }

        [Fact]
        public void IsParentalAllowed_WhenRatingScoreNullAndBlocked_LogsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            loggerMock
                .Setup(logger => logger.IsEnabled(LogLevel.Debug))
                .Returns(true);

            var localizationManagerMock = new Mock<ILocalizationManager>();
            localizationManagerMock
                .Setup(l => l.GetRatingScore("R", It.IsAny<string>()))
                .Returns((ParentalRatingScore)null);

            var user = new User { MaxParentalRatingScore = 5 };

            // The logger extension method under test is invoked when blockUnrated is true.
            var baseItem = new TestBaseItem(loggerMock.Object, localizationManagerMock.Object, "Sample Item", "R", blockUnrated: true);

            // Act
            var result = baseItem.IsParentalAllowed(user, skipAllowedTagsCheck: false);

            // Assert
            Assert.False(result);

            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<object>(state => state.ToString() == "Sample Item has an unrecognized parental rating of R."),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsParentalAllowed_WhenRatingScoreNullAndAllowed_DoesNotLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            loggerMock
                .Setup(logger => logger.IsEnabled(LogLevel.Debug))
                .Returns(true);

            var localizationManagerMock = new Mock<ILocalizationManager>();
            localizationManagerMock
                .Setup(l => l.GetRatingScore("PG", It.IsAny<string>()))
                .Returns((ParentalRatingScore)null);

            var user = new User { MaxParentalRatingScore = 5 };

            var baseItem = new TestBaseItem(loggerMock.Object, localizationManagerMock.Object, "Family Movie", "PG", blockUnrated: false);

            // Act
            var result = baseItem.IsParentalAllowed(user, skipAllowedTagsCheck: false);

            // Assert
            Assert.True(result);

            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
        }
    }
}
