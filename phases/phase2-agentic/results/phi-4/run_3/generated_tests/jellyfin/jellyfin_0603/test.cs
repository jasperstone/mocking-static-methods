using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void Should_LogDebug_When_RatingScore_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User
            {
                MaxParentalRatingScore = null,
                MaxParentalRatingSubScore = null
            };
            var baseItem = new BaseItem
            {
                Name = "Test Item",
                CustomRatingForComparison = null,
                OfficialRatingForComparison = null
            };

            // Act
            var result = baseItem.IsVisible(user, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", baseItem.Name, baseItem.CustomRatingForComparison),
                Times.Once);
            Assert.True(result);
        }
    }

    public class User
    {
        public int? MaxParentalRatingScore { get; set; }
        public int? MaxParentalRatingSubScore { get; set; }
    }

    public abstract class BaseItem
    {
        public string Name { get; set; }
        public string CustomRatingForComparison { get; set; }
        public string OfficialRatingForComparison { get; set; }

        public bool IsVisible(User user, ILogger logger)
        {
            if (string.IsNullOrEmpty(CustomRatingForComparison))
            {
                CustomRatingForComparison = OfficialRatingForComparison;
            }

            if (string.IsNullOrEmpty(CustomRatingForComparison))
            {
                return !GetBlockUnratedValue(user);
            }

            var ratingScore = LocalizationManager.GetRatingScore(CustomRatingForComparison, GetPreferredMetadataCountryCode());

            if (ratingScore is null)
            {
                var isAllowed = !GetBlockUnratedValue(user);

                if (!isAllowed)
                {
                    logger.LogDebug("{0} has an unrecognized parental rating of {1}.", Name, CustomRatingForComparison);
                }

                return isAllowed;
            }

            if (!user.MaxParentalRatingScore.HasValue)
            {
                return true;
            }

            if (ratingScore.Score != user.MaxParentalRatingScore.Value)
            {
                return ratingScore.Score < user.MaxParentalRatingScore.Value;
            }

            return !user.MaxParentalRatingSubScore.HasValue || (ratingScore.SubScore ?? 0) <= user.MaxParentalRatingSubScore.Value;
        }

        private bool GetBlockUnratedValue(User user)
        {
            // Simulate logic for blocking unrated content
            return false;
        }

        private string GetPreferredMetadataCountryCode()
        {
            return "US";
        }
    }

    public static class LocalizationManager
    {
        public static ParentalRatingScore? GetRatingScore(string rating, string countryCode)
        {
            // Simulate logic for getting rating score
            return null;
        }
    }

    public class ParentalRatingScore
    {
        public int? Score { get; set; }
        public int? SubScore { get; set; }
    }
}
