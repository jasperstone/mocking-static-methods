using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public override string CustomRatingForComparison { get; set; }
            public override string OfficialRatingForComparison { get; set; }
            public override ILogger Logger { get; set; }
            public override ILocalizationManager LocalizationManager { get; set; }
            public override string GetPreferredMetadataCountryCode() => "US";
            public override bool GetBlockUnratedValue(User user) => false;
            public override bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck) => true;

            public bool CallIsUserAllowed(User user, bool skipAllowedTagsCheck)
            {
                return IsUserAllowed(user, skipAllowedTagsCheck);
            }
        }

        private class User
        {
            public int? MaxParentalRatingScore { get; set; }
            public int? MaxParentalRatingSubScore { get; set; }
        }

        private class ParentalRatingScore
        {
            public int Score { get; set; }
            public int? SubScore { get; set; }
        }

        private interface ILocalizationManager
        {
            ParentalRatingScore GetRatingScore(string rating, string countryCode);
        }

        [Fact]
        public void IsUserAllowed_LogsDebug_WhenRatingScoreIsNull_AndBlockUnratedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var localizationManagerMock = new Mock<ILocalizationManager>();
            var user = new User { MaxParentalRatingScore = 5, MaxParentalRatingSubScore = 2 };
            var item = new TestBaseItem
            {
                Name = "TestItem",
                Logger = loggerMock.Object,
                LocalizationManager = localizationManagerMock.Object,
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = "OfficialRating"
            };

            // Setup LocalizationManager to return null rating score
            localizationManagerMock.Setup(lm => lm.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((ParentalRatingScore)null);

            // Override GetBlockUnratedValue to return true to trigger logging
            item.GetBlockUnratedValue = (User u) => true;

            // Act
            var result = item.CallIsUserAllowed(user, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestItem has an unrecognized parental rating of CustomRating.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
