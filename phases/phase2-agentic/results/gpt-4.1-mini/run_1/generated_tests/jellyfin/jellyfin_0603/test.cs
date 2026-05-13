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
            public Func<User, bool, bool> IsVisibleViaTagsFunc { get; set; }
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

            // Expose the method to test
            public bool CallIsItemAllowedForUser(User user, bool skipAllowedTagsCheck)
            {
                return IsItemAllowedForUser(user, skipAllowedTagsCheck);
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
        public void IsItemAllowedForUser_LogsDebug_WhenRatingScoreIsNullAndBlockUnratedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var user = new User();
            var item = new TestBaseItem
            {
                Logger = loggerMock.Object,
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => true,
                GetRatingScoreFunc = (rating, country) => null
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestItem has an unrecognized parental rating of custom.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsItemAllowedForUser_ReturnsTrue_WhenMaxAllowedRatingIsNull()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = null };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 1, SubScore = 0 }
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsItemAllowedForUser_ReturnsFalse_WhenRatingScoreScoreIsGreaterThanMaxAllowedRating()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 2 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 0 }
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsItemAllowedForUser_ReturnsTrue_WhenRatingScoreScoreIsLessThanMaxAllowedRating()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 3 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 2, SubScore = 0 }
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsItemAllowedForUser_ReturnsTrue_WhenRatingScoreScoreEqualsMaxAllowedRatingAndSubScoreIsLessOrEqual()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 3, MaxParentalRatingSubScore = 1 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 1 }
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsItemAllowedForUser_ReturnsFalse_WhenRatingScoreScoreEqualsMaxAllowedRatingAndSubScoreIsGreater()
        {
            // Arrange
            var user = new User { MaxParentalRatingScore = 3, MaxParentalRatingSubScore = 1 };
            var item = new TestBaseItem
            {
                Logger = Mock.Of<ILogger>(),
                Name = "TestItem",
                CustomRatingForComparison = "custom",
                OfficialRatingForComparison = null,
                IsVisibleViaTagsFunc = (u, skip) => true,
                GetBlockUnratedValueFunc = u => false,
                GetRatingScoreFunc = (rating, country) => new ParentalRatingScore { Score = 3, SubScore = 2 }
            };

            // Act
            var result = item.CallIsItemAllowedForUser(user, false);

            // Assert
            Assert.False(result);
        }
    }
}
