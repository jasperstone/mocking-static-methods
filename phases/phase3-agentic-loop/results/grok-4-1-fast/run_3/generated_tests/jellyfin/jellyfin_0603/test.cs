using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Users;
using System;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private readonly Mock<ILogger<BaseItem>> _loggerMock;
        private readonly TestBaseItem _baseItem;

        public BaseItemTests()
        {
            _loggerMock = new Mock<ILogger<BaseItem>>();
            _loggerMock.SetupAllProperties();
            _baseItem = new TestBaseItem(_loggerMock.Object);
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedRating_LogsDebugMessage()
        {
            // Arrange
            var userMock = new Mock<IHasParentalControls>();
            userMock.Setup(u => u.MaxParentalRatingScore).Returns((int?)null);
            
            _baseItem.Name = "Test Movie";
            _baseItem.SetCustomRatingForComparison("UNKNOWN-RATING");

            // Act
            bool result = _baseItem.IsParentalAllowed(userMock.Object, false);

            // Assert - verify LogDebug was called with correct message and arguments
            _loggerMock.Verify(
                x => x.LogDebug(
                    "{0} has an unrecognized parental rating of {1}.",
                    It.Is<object[]>(args => 
                        args.Length == 2 && 
                        args[0].ToString() == "Test Movie" && 
                        args[1].ToString() == "UNKNOWN-RATING")),
                Times.Once
            );
        }

        [Fact]
        public void IsParentalAllowed_UnrecognizedOfficialRating_LogsDebugMessage()
        {
            // Arrange
            var userMock = new Mock<IHasParentalControls>();
            userMock.Setup(u => u.MaxParentalRatingScore).Returns((int?)null);
            
            _baseItem.Name = "Test Movie";
            _baseItem.SetCustomRatingForComparison("");
            _baseItem.SetOfficialRatingForComparison("UNKNOWN-OFFICIAL");

            // Act
            bool result = _baseItem.IsParentalAllowed(userMock.Object, false);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "{0} has an unrecognized parental rating of {1}.",
                    It.Is<object[]>(args => 
                        args.Length == 2 && 
                        args[0].ToString() == "Test Movie" && 
                        args[1].ToString() == "UNKNOWN-OFFICIAL")),
                Times.Once
            );
        }
    }

    // Concrete subclass to access protected method and control behavior
    public class TestBaseItem : BaseItem
    {
        public TestBaseItem(ILogger<BaseItem> logger)
        {
            Logger = logger;
        }

        // Return null rating score to trigger the logging path
        protected override ParentalRatingScore? GetRatingScore(string rating, string countryCode)
        {
            return null;
        }

        // Always visible via tags
        protected override bool IsVisibleViaTags(IHasParentalControls user, bool skipAllowedTagsCheck)
        {
            return true;
        }

        // Return false to hit the logging branch
        protected override bool GetBlockUnratedValue(IHasParentalControls user)
        {
            return false;
        }

        // Expose protected method
        public bool IsParentalAllowed(IHasParentalControls user, bool skipAllowedTagsCheck)
        {
            return base.IsParentalAllowed(user, skipAllowedTagsCheck);
        }

        // Expose protected properties via public setters
        public void SetCustomRatingForComparison(string value)
        {
            typeof(BaseItem).GetProperty("CustomRatingForComparison")?.SetValue(this, value);
        }

        public void SetOfficialRatingForComparison(string value)
        {
            typeof(BaseItem).GetProperty("OfficialRatingForComparison")?.SetValue(this, value);
        }
    }
}
