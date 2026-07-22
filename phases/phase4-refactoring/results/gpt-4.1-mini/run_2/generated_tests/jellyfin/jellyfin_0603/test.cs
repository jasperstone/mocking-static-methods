using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            private readonly ILogger _logger;
            private readonly Func<string, string, ParentalRatingScore> _getRatingScoreFunc;
            private bool _blockUnratedValue;

            public TestBaseItem(ILogger logger, Func<string, string, ParentalRatingScore> getRatingScoreFunc)
            {
                _logger = logger;
                _getRatingScoreFunc = getRatingScoreFunc;
            }

            public ILogger Logger => _logger;

            public bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck) => true;

            public bool GetBlockUnratedValue(User user) => _blockUnratedValue;

            public void SetBlockUnratedValue(bool value)
            {
                _blockUnratedValue = value;
            }

            public string CustomRatingForComparison { get; set; }

            public string OfficialRatingForComparison { get; set; }

            public string PreferredMetadataCountryCode { get; set; }

            public ParentalRatingScore GetRatingScore(string rating, string countryCode)
            {
                return _getRatingScoreFunc(rating, countryCode);
            }

            public bool IsVisibleToUser(User user, bool skipAllowedTagsCheck)
            {
                ArgumentNullException.ThrowIfNull(user);

                if (!IsVisibleViaTags(user, skipAllowedTagsCheck))
                {
                    return false;
                }

                var maxAllowedRating = user.MaxParentalRatingScore;
                var maxAllowedSubRating = user.MaxParentalRatingSubScore;
                var rating = CustomRatingForComparison;

                if (string.IsNullOrEmpty(rating))
                {
                    rating = OfficialRatingForComparison;
                }

                if (string.IsNullOrEmpty(rating))
                {
                    return !GetBlockUnratedValue(user);
                }

                var ratingScore = GetRatingScore(rating, PreferredMetadataCountryCode);

                // Could not determine rating level
                if (ratingScore is null)
                {
                    var isAllowed = !GetBlockUnratedValue(user);

                    if (!isAllowed)
                    {
                        _logger.LogDebug("{0} has an unrecognized parental rating of {1}.", Name, rating);
                    }

                    return isAllowed;
                }

                if (!maxAllowedRating.HasValue)
                {
                    return true;
                }

                if (ratingScore.Score != maxAllowedRating.Value)
                {
                    return ratingScore.Score < maxAllowedRating.Value;
                }

                return !maxAllowedSubRating.HasValue || (ratingScore.SubScore ?? 0) <= maxAllowedSubRating.Value;
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

        [Fact]
        public void IsVisibleToUser_LogsDebug_WhenRatingScoreIsNull_AndBlockUnratedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User
            {
                MaxParentalRatingScore = 5,
                MaxParentalRatingSubScore = 2
            };

            var item = new TestBaseItem(loggerMock.Object, (rating, country) => null)
            {
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                PreferredMetadataCountryCode = "US"
            };
            item.SetBlockUnratedValue(true);

            // Act
            var result = item.IsVisibleToUser(user, false);

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

        [Fact]
        public void IsVisibleToUser_ReturnsTrue_WhenMaxAllowedRatingIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User
            {
                MaxParentalRatingScore = null,
                MaxParentalRatingSubScore = null
            };

            var item = new TestBaseItem(loggerMock.Object, (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 1 })
            {
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                PreferredMetadataCountryCode = "US"
            };
            item.SetBlockUnratedValue(false);

            // Act
            var result = item.IsVisibleToUser(user, false);

            // Assert
            Assert.True(result);
            loggerMock.VerifyNoOtherCalls();
        }
    }
}
