using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;
using System;

namespace MediaBrowser.Tests
{
    public class BaseItemTests
    {
        private class TestItem : BaseItem
        {
            public string NameForRating { get; set; }
            public string Rating { get; set; }
            public Func<string, string> GetPreferredMetadataCountryCodeFunc { get; set; }
            public Func<string, RatingScore> GetRatingScoreFunc { get; set; }
            public Func<string, string> GetOfficialRatingForComparison { get; set; }
            public Func<bool> GetBlockUnratedValue { get; set; }
            public ILogger Logger { get; set; }

            public override string Name => NameForRating;
            public override string OfficialRatingForComparison => GetOfficialRatingForComparison();
            public override string CustomRatingForComparison => Rating;
            public override string GetPreferredMetadataCountryCode() => GetPreferredMetadataCountryCodeFunc?.Invoke(null);
            public override RatingScore LocalizationManager_GetRatingScore(string rating, string countryCode) => GetRatingScoreFunc?.Invoke(rating);
            public override bool GetBlockUnratedValue() => GetBlockUnratedValue?.Invoke() ?? false;

            public TestItem(ILogger logger)
            {
                Logger = logger;
            }
        }

        [Fact]
        public void LogDebug_IsCalled_WhenRatingScoreIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var item = new TestItem(loggerMock.Object)
            {
                NameForRating = "TestItem",
                Rating = "UnrecognizedRating",
                GetRatingScoreFunc = rating => null,
                GetOfficialRatingForComparison = () => null,
                GetPreferredMetadataCountryCodeFunc = _ => "US",
                GetBlockUnratedValue = () => false
            };

            // Act
            var result = item.IsVisibleViaTags(new object(), false);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}
