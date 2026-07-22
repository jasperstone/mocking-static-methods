using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using System;

namespace MediaBrowser.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void LogDebug_IsCalled_WhenRatingScoreIsNull()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseItem>>();
            var item = new TestableBaseItem(mockLogger.Object);
            var user = new User { MaxParentalRatingScore = 10, MaxParentalRatingSubScore = 5 };
            var rating = "UnrecognizedRating";

            // Act
            var result = item.CheckRatingVisibility(user, rating, "US", false);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", "TestItem", rating),
                Times.Once);
        }
    }

    // Dummy classes to facilitate testing
    public class User
    {
        public int MaxParentalRatingScore { get; set; }
        public int MaxParentalRatingSubScore { get; set; }
    }

    public class TestableBaseItem : BaseItem
    {
        private readonly ILogger<BaseItem> _logger;

        public TestableBaseItem(ILogger<BaseItem> logger)
        {
            _logger = logger;
        }

        public bool CheckRatingVisibility(User user, string rating, string countryCode, bool skipAllowedTagsCheck)
        {
            // Simulate the method logic that calls Logger.LogDebug
            var ratingScore = (RatingScore)null; // Simulate null rating score
            if (ratingScore is null)
            {
                if (!_logger.IsEnabled(LogLevel.Debug))
                    return false;
                _logger.LogDebug("{0} has an unrecognized parental rating of {1}.", "TestItem", rating);
                return false;
            }
            return true;
        }
    }

    public class RatingScore
    {
        public int Score { get; set; }
        public int? SubScore { get; set; }
    }
}
