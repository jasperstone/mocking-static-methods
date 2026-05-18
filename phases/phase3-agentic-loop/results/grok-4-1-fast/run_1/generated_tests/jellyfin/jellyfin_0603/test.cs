using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemParentalRatingTests
    {
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly TestBaseItem _baseItem;

        public BaseItemParentalRatingTests()
        {
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _baseItem = new TestBaseItem(_localizationManagerMock.Object);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_BlocksUnrated_LogsDebugMessage()
        {
            // Arrange
            var user = new UserDto
            {
                MaxParentalRatingScore = 5m
            };
            _baseItem.BlockUnratedItems = true;
            _baseItem.OfficialRatingForComparison = "UNKNOWN";
            
            _localizationManagerMock
                .Setup(m => m.GetRatingScore("UNKNOWN", "US"))
                .Returns((ParentalRatingScore?)null);

            // Act
            var result = _baseItem.CallIsParentalAllowed(user, false);

            // Assert
            Assert.False(result);
            Assert.Single(_baseItem.DebugLogMessages);
            Assert.Contains("Test Item has an unrecognized parental rating of UNKNOWN.", _baseItem.DebugLogMessages[0]);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_AllowsUnrated_DoesNotLogDebugMessage()
        {
            // Arrange
            var user = new UserDto
            {
                MaxParentalRatingScore = 5m
            };
            _baseItem.BlockUnratedItems = false;
            _baseItem.OfficialRatingForComparison = "UNKNOWN";
            
            _localizationManagerMock
                .Setup(m => m.GetRatingScore("UNKNOWN", "US"))
                .Returns((ParentalRatingScore?)null);

            // Act
            var result = _baseItem.CallIsParentalAllowed(user, false);

            // Assert
            Assert.True(result);
            Assert.Empty(_baseItem.DebugLogMessages);
        }
    }

    public class TestBaseItem : BaseItem
    {
        public List<string> DebugLogMessages { get; } = new();
        public bool BlockUnratedItems { get; set; }
        public string OfficialRatingForComparison { get; set; } = string.Empty;
        public string CustomRatingForComparison { get; set; } = string.Empty;
        private readonly ILocalizationManager _localizationManager;

        public TestBaseItem(ILocalizationManager localizationManager)
        {
            _localizationManager = localizationManager;
            Name = "Test Item";
            PreferredMetadataCountryCode = "US";
            Logger = NullLogger<BaseItem>.Instance;
        }

        // Public wrapper for protected method
        public bool CallIsParentalAllowed(UserDto user, bool skipAllowedTagsCheck)
        {
            return IsParentalAllowed(user, skipAllowedTagsCheck);
        }

        // Override the actual LogDebug extension method call by intercepting it
        protected new void LogDebug(string message, params object[] args)
        {
            DebugLogMessages.Add(string.Format(message, args));
        }

        protected override bool GetBlockUnratedValue(UserDto user) => !BlockUnratedItems;

        public override string OfficialRatingForComparison => OfficialRatingForComparison;

        public override string CustomRatingForComparison => CustomRatingForComparison;
    }
}
