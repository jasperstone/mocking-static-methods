using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void Should_LogDebug_When_RatingScore_Is_Null()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
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
            var result = baseItem.IsVisibleViaTags(user, skipAllowedTagsCheck: false);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(It.Is<string>(s => s.Contains("Test Item") && s.Contains("unrecognized parental rating")),
                It.IsAny<object[]>()),
                Times.Once);
        }
    }

    // Mock User class for testing purposes
    public class User
    {
        public int? MaxParentalRatingScore { get; set; }
        public int? MaxParentalRatingSubScore { get; set; }
    }
}
