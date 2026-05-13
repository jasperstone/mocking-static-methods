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
            public override ILogger Logger { get; set; }
            public Func<string, bool, bool> IsVisibleViaTagsFunc { get; set; }
            public Func<User, bool> GetBlockUnratedValueFunc { get; set; }
            public Func<string, string, ParentalRatingScore> GetRatingScoreFunc { get; set; }
            public override string CustomRatingForComparison { get; set; }
            public override string OfficialRatingForComparison { get; set; }
            public override string Name { get; set; }
            public override string GetPreferredMetadataCountryCode() => "US";

            public override bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck)
            {
                return IsVisibleViaTagsFunc?.Invoke(user, skipAllowedTagsCheck) ?? true;
            }

            public override bool GetBlockUnratedValue(User user)
            {
                return GetBlockUnratedValueFunc?.Invoke(user) ?? false;
            }

            public override ParentalRatingScore LocalizationManager_GetRatingScore(string rating, string countryCode)
            {
                return GetRatingScoreFunc?.Invoke(rating, countryCode);
            }

            // We expose the method under test here for testing
            public bool TestIsUserAllowed(User user, bool skipAllowedTagsCheck)
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

        [Fact]
        public void IsUserAllowed_LogsDebug_WhenRatingScoreIsNullAndBlockUnratedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User { MaxParentalRatingScore = 5, MaxParentalRatingSubScore = 2 };
            var item = new TestBaseItem
            {
                Logger = loggerMock.Object,
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => true,
                GetRatingScoreFunc = (rating, country) => null
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

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
        public void IsUserAllowed_ReturnsTrue_WhenMaxAllowedRatingIsNull()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = null };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 1 }
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsUserAllowed_ReturnsFalse_WhenRatingScoreScoreIsGreaterThanMaxAllowedRating()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 3 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 5, SubScore = 1 }
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsUserAllowed_ReturnsTrue_WhenRatingScoreScoreIsLessThanMaxAllowedRating()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 5 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 1 }
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsUserAllowed_ReturnsTrue_WhenRatingScoreScoreEqualsMaxAllowedRatingAndSubScoreIsLessOrEqual()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 5, MaxParentalRatingSubScore = 2 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 5, SubScore = 2 }
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsUserAllowed_ReturnsFalse_WhenRatingScoreScoreEqualsMaxAllowedRatingAndSubScoreIsGreater()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 5, MaxParentalRatingSubScore = 2 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "CustomRating",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 5, SubScore = 3 }
            };

            // Act
            var result = item.TestIsUserAllowed(user, false);

            // Assert
            Assert.False(result);
        }
    }
}
