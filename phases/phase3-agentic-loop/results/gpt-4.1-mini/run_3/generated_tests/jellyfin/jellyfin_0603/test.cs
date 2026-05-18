using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            private readonly ILogger _logger;
            private readonly ILocalizationManager _localizationManager;

            public TestBaseItem(ILogger logger, ILocalizationManager localizationManager)
            {
                _logger = logger;
                _localizationManager = localizationManager;
            }

            protected override ILogger Logger => _logger;

            protected override ILocalizationManager LocalizationManager => _localizationManager;

            // Expose the method to test parental rating visibility
            public bool CallIsVisibleByParentalRating(User user, bool skipAllowedTagsCheck)
            {
                return IsVisibleByParentalRating(user, skipAllowedTagsCheck);
            }
        }

        private class User
        {
            public int? MaxParentalRatingScore { get; set; }
            public int? MaxParentalRatingSubScore { get; set; }
        }

        [Fact]
        public void IsVisibleByParentalRating_LogsDebug_WhenUnrecognizedRating()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var user = new User
            {
                MaxParentalRatingScore = 5,
                MaxParentalRatingSubScore = null
            };

            var baseItem = new TestBaseItem(loggerMock.Object, localizationManagerMock.Object);
            baseItem.Name = "TestItem";
            baseItem.SetCustomRatingForComparison("UnrecognizedRating");
            baseItem.SetOfficialRatingForComparison(null);

            // LocalizationManager.GetRatingScore returns null to simulate unrecognized rating
            localizationManagerMock.Setup(lm => lm.GetRatingScore("UnrecognizedRating", It.IsAny<string>()))
                .Returns((ParentalRatingScore)null);

            // Act
            var result = baseItem.CallIsVisibleByParentalRating(user, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestItem has an unrecognized parental rating of UnrecognizedRating.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
