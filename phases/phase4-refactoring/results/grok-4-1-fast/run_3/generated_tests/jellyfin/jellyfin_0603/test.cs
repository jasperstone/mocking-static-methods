#nullable enable

using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemParentalRatingTests
    {
        private readonly Mock<ILogger<BaseItem>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly TestBaseItem _baseItem;

        public BaseItemParentalRatingTests()
        {
            _loggerMock = new Mock<ILogger<BaseItem>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _baseItem = new TestBaseItem(_loggerMock.Object, _localizationManagerMock.Object);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_LogsDebugMessage()
        {
            // Arrange
            var user = new UserDto { Id = Guid.NewGuid() };

            _localizationManagerMock
                .Setup(x => x.GetRatingScore("UNKNOWN-RATING", "US"))
                .Returns((MediaBrowser.Model.Entities.ParentalRatingScore?)null);

            _baseItem.SetBlockUnratedValue(true);

            // Act
            var result = _baseItem.IsParentalAllowed(user);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", "Test Item", "UNKNOWN-RATING"),
                Times.Once);

            Assert.False(result);
        }

        [Fact]
        public void IsParentalAllowed_NullRatingScoreWithBlockUnratedFalse_NoLog()
        {
            // Arrange
            var user = new UserDto { Id = Guid.NewGuid() };

            _localizationManagerMock
                .Setup(x => x.GetRatingScore(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((MediaBrowser.Model.Entities.ParentalRatingScore?)null);

            _baseItem.SetBlockUnratedValue(false);

            // Act
            var result = _baseItem.IsParentalAllowed(user);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);

            Assert.True(result);
        }

        private class TestBaseItem : BaseItem
        {
            private readonly ILogger<BaseItem> _logger;
            private readonly ILocalizationManager _localizationManager;
            private bool _blockUnratedValue = true;

            public TestBaseItem(ILogger<BaseItem> logger, ILocalizationManager localizationManager)
            {
                _logger = logger;
                _localizationManager = localizationManager;
                Logger = _logger;
                Id = Guid.NewGuid();
                Name = "Test Item";
            }

            public bool IsParentalAllowed(UserDto user) => base.IsParentalAllowed(user, false);

            public void SetBlockUnratedValue(bool value) => _blockUnratedValue = value;

            public new string Name { get; set; } = default!;

            public new Guid Id { get; set; }

            public bool IsVisibleViaTags(UserDto user, bool skipAllowedTagsCheck) => true;

            public bool GetBlockUnratedValue(UserDto user) => _blockUnratedValue;

            public new string GetPreferredMetadataCountryCode() => "US";
        }
    }
}
