using System;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem()
            {
                Name = "TestItem";
                CustomRatingForComparison = "UnknownRating";
                OfficialRatingForComparison = null;
                UserData = new System.Collections.Generic.List<UserData>();
            }

            // Expose the method for testing
            public bool CallIsVisibleViaParentalRatingScore(User user, bool skipAllowedTagsCheck)
            {
                return IsVisibleViaParentalRatingScore(user, skipAllowedTagsCheck);
            }

            // Override GetBlockUnratedValue to simulate block unrated behavior
            protected bool GetBlockUnratedValue(User user)
            {
                return false; // simulate block unrated = false
            }
        }

        private class User
        {
            public int? MaxParentalRatingScore { get; set; }
            public int? MaxParentalRatingSubScore { get; set; }
        }

        [Fact]
        public void IsVisibleViaParentalRatingScore_ReturnsFalse_WhenRatingScoreNull_AndBlockUnratedFalse()
        {
            // Arrange
            var item = new TestBaseItem();
            var user = new User
            {
                MaxParentalRatingScore = 5,
                MaxParentalRatingSubScore = 2
            };

            // Act
            var result = item.CallIsVisibleViaParentalRatingScore(user, false);

            // Assert
            Assert.False(result);
        }
    }
}
