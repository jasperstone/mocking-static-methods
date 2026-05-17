using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public override string CustomRatingForComparison { get; set; }
            public override string OfficialRatingForComparison { get; set; }
            public override string Name { get; set; }
            public override string GetPreferredMetadataCountryCode() => "US";

            public bool BlockUnratedValue { get; set; }

            public bool IsVisibleViaTagsResult { get; set; } = true;

            public override bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck) => IsVisibleViaTagsResult;

            public bool GetBlockUnratedValue(User user) => BlockUnratedValue;

            public bool CallIsVisible(User user, bool skipAllowedTagsCheck) => IsVisible(user, skipAllowedTagsCheck);
        }

        private class User
        {
            public int? MaxParentalRatingScore { get; set; }
            public int? MaxParentalRatingSubScore { get; set; }
        }

        private class LocalizationManager : ILocalizationManager
        {
            public ParentalRatingScore GetRatingScore(string rating, string countryCode)
            {
                if (rating == "recognized")
                {
                    return new ParentalRatingScore { Score = 5, SubScore = 1 };
                }
                if (rating == "unrecognized")
                {
                    return null;
                }
                return null;
            }
        }

        private class ParentalRatingScore
        {
            public int Score { get; set; }
            public int? SubScore { get; set; }
        }

        [Fact]
        public void IsVisible_ReturnsFalse_WhenUnrecognizedRatingAndBlockUnratedTrue()
        {
            // Arrange
            var item = new TestBaseItem
            {
                Name = "TestItem",
                CustomRatingForComparison = "unrecognized",
                OfficialRatingForComparison = null,
                BlockUnratedValue = true
            };

            var user = new User
            {
                MaxParentalRatingScore = 10,
                MaxParentalRatingSubScore = 5
            };

            // Act
            var result = item.CallIsVisible(user, false);

            // Assert
            Assert.False(result);
        }
    }
}
