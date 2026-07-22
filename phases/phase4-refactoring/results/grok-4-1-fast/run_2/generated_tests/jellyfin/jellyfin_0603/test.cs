using System;
using System.Collections.Generic;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Entities;
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
        private readonly TestBaseItem _baseItem;

        public BaseItemTests()
        {
            _loggerMock = new Mock<ILogger<BaseItem>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _baseItem = new TestBaseItem(_loggerMock.Object, _localizationManagerMock.Object);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_LogsDebugMessage()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                MaxParentalRatingScore = 5
            };

            _localizationManagerMock
                .Setup(x => x.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((ParentalRatingScore?)null);

            // Act
            var result = _baseItem.CallIsParentalAllowed(user, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", _baseItem.Name, It.IsAny<string>()),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void IsParentalAllowed_ValidRatingScore_DoesNotLogDebug()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var ratingScore = new ParentalRatingScore(3, null);

            _localizationManagerMock
                .Setup(x => x.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ratingScore);

            // Act
            var result = _baseItem.CallIsParentalAllowed(user, false);

            // Assert - No debug log should be called
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);

            Assert.True(result);
        }

        private class TestBaseItem : BaseItem
        {
            private readonly ILogger<BaseItem> _logger;
            private readonly ILocalizationManager _localizationManager;

            public TestBaseItem(ILogger<BaseItem> logger, ILocalizationManager localizationManager)
            {
                _logger = logger;
                _localizationManager = localizationManager;
                Name = "TestItem";
                PreferredMetadataCountryCode = "US";
                Logger = _logger;
            }

            public ILogger<BaseItem> Logger { get; set; }

            public bool CallIsParentalAllowed(User user, bool skipAllowedTagsCheck)
            {
                return IsParentalAllowed(user, skipAllowedTagsCheck);
            }

            // Return values that allow the method to reach the logging point
            protected override bool IsVisibleViaTags(User user, bool skipAllowedTagsCheck) => true;
            protected override bool GetBlockUnratedValue(User user) => true;
        }
    }
}
