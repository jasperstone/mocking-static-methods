using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var localizationMock = new Mock<ILocalizationManager>();
            localizationMock.Setup(x => x.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                           .Returns((ParentalRatingScore?)null);

            var item = new TestBaseItem(loggerMock.Object, localizationMock.Object);
            item.Name = "Test Item";
            item.CustomRatingForComparison = "UNKNOWN_RATING";

            // Act
            item.IsParentalAllowed(new TestUser());

            // Assert - verify LogDebug extension was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Test Item") && 
                        v.ToString()!.Contains("UNKNOWN_RATING")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRatingNotBlocked_DoesNotLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var localizationMock = new Mock<ILocalizationManager>();
            localizationMock.Setup(x => x.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                           .Returns((ParentalRatingScore?)null);

            var item = new TestBaseItem(loggerMock.Object, localizationMock.Object, blockUnrated: false);
            item.Name = "Test Item";
            item.CustomRatingForComparison = "UNKNOWN_RATING";

            // Act
            item.IsParentalAllowed(new TestUser());

            // Assert - no log when isAllowed = true
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private class TestBaseItem : BaseItem
        {
            private readonly ILocalizationManager _localizationManager;
            private readonly bool _blockUnrated;

            public TestBaseItem(ILogger<BaseItem> logger, ILocalizationManager localizationManager, bool blockUnrated = true)
            {
                Logger = logger;
                _localizationManager = localizationManager;
                _blockUnrated = blockUnrated;
            }

            public override bool IsVisibleViaTags(object user, bool skipAllowedTagsCheck) => true;

            protected override bool GetBlockUnratedValue(object user) => _blockUnrated;

            protected override ILocalizationManager LocalizationManager => _localizationManager;

            public override string GetPreferredMetadataCountryCode() => "US";
        }

        private class TestUser
        {
            public int? MaxParentalRatingScore { get; set; }
        }
    }
}
