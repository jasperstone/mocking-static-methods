using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsParentalAllowed_LogsDebugForUnrecognizedRating_WhenRatingScoreIsNullAndBlockUnratedIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BaseItem>>();
            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message, args) =>
            {
                Assert.Equal("{0} has an unrecognized parental rating of {1}.", message);
                Assert.Equal("Test Item", args[0]);
                Assert.Equal("UNKNOWN_RATING", args[1]);
            });

            var mockLocalizationManager = new Mock<ILocalizationManager>();
            mockLocalizationManager.Setup(lm => lm.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((ParentalRatingScore?)null);

            var user = new Mock<User>();
            user.Setup(u => u.MaxParentalRatingScore).Returns((int?)null);
            user.Setup(u => u.MaxParentalRatingSubScore).Returns((int?)null);

            var baseItem = new TestBaseItem
            {
                Name = "Test Item",
                OfficialRatingForComparison = "UNKNOWN_RATING",
                CustomRatingForComparison = null,
                Logger = mockLogger.Object,
                LocalizationManager = mockLocalizationManager.Object
            };

            // Act
            var result = baseItem.IsParentalAllowed(user.Object, false);

            // Assert
            mockLogger.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.False(result);
        }

        private class TestBaseItem : BaseItem
        {
            public new ILogger Logger { get; set; } = NullLogger<BaseItem>.Instance;
            public new ILocalizationManager LocalizationManager { get; set; } = NullLocalizationManager.Instance;

            public bool IsVisibleViaTagsResult { get; set; } = true;
            public bool GetBlockUnratedValueResult { get; set; } = true;

            public override bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck) => IsVisibleViaTagsResult;

            public override bool GetBlockUnratedValue(User user) => GetBlockUnratedValueResult;

            public override string GetPreferredMetadataCountryCode() => "US";
        }
    }
}
