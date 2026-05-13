using System;
using System.Collections.Generic;
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
        private readonly Mock<ILogger<BaseItem>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly BaseItem _baseItem;

        public BaseItemTests()
        {
            _loggerMock = new Mock<ILogger<BaseItem>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();

            // Create a concrete subclass to test protected method
            _baseItem = new TestBaseItem(_loggerMock.Object, _localizationManagerMock.Object)
            {
                Name = "Test Item",
                PreferredMetadataCountryCode = "US"
            };
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_LogsDebugMessage()
        {
            // Arrange
            var user = new UserDto
            {
                MaxParentalRatingScore = 5,
                BlockUnratedItems = new List<string>()
            };

            _localizationManagerMock
                .Setup(m => m.GetRatingScore(It.IsAny<string>(), "US"))
                .Returns((ParentalRatingScore?)null);

            // Act
            var result = _baseItem.IsParentalAllowed(user, false);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Test Item has an unrecognized parental rating of")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void IsParentalAllowed_ValidRating_NoDebugLog()
        {
            // Arrange
            var user = new UserDto
            {
                MaxParentalRatingScore = 5,
                BlockUnratedItems = new List<string>()
            };

            _localizationManagerMock
                .Setup(m => m.GetRatingScore("PG", "US"))
                .Returns(new ParentalRatingScore { Score = 3 });

            _baseItem.OfficialRatingForComparison = "PG";

            // Act
            var result = _baseItem.IsParentalAllowed(user, false);

            // Assert - No debug log should be called
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.True(result);
        }

        private static bool ContainsMessage<TState>(TState state, string expectedMessage)
        {
            return state?.ToString()?.Contains(expectedMessage) == true;
        }

        private class TestBaseItem : BaseItem
        {
            private readonly ILogger<BaseItem> _logger;
            private readonly ILocalizationManager _localizationManager;

            public TestBaseItem(ILogger<BaseItem> logger, ILocalizationManager localizationManager)
            {
                _logger = logger;
                _localizationManager = localizationManager;
            }

            public new bool IsParentalAllowed(UserDto user, bool skipAllowedTagsCheck = false)
            {
                return base.IsParentalAllowed(user, skipAllowedTagsCheck);
            }

            protected override ILocalizationManager LocalizationManager => _localizationManager;
            public override ILogger Logger => _logger;
        }
    }
}
