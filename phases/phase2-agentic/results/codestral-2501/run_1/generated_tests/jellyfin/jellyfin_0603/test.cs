using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<BaseItem> _mockBaseItem;

        public BaseItemTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockBaseItem = new Mock<BaseItem>();
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNull()
        {
            // Arrange
            User user = null;

            // Act
            var result = _mockBaseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnFalse_WhenUserIsNotVisible()
        {
            // Arrange
            var user = new User
            {
                // Set properties to make the user not visible
            };

            // Act
            var result = _mockBaseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVisibleViaTags_ShouldReturnTrue_WhenUserIsVisible()
        {
            // Arrange
            var user = new User
            {
                // Set properties to make the user visible
            };

            // Act
            var result = _mockBaseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void LogDebug_ShouldBeCalled_WhenRatingIsUnrecognized()
        {
            // Arrange
            var user = new User
            {
                // Set properties to make the user visible
            };

            _mockBaseItem.Setup(x => x.Name).Returns("TestItem");
            _mockBaseItem.Setup(x => x.CustomRatingForComparison).Returns("UnrecognizedRating");
            _mockBaseItem.Setup(x => x.OfficialRatingForComparison).Returns("UnrecognizedRating");
            _mockBaseItem.Setup(x => x.GetBlockUnratedValue(user)).Returns(false);
            _mockBaseItem.Setup(x => x.Logger).Returns(_mockLogger.Object);

            // Act
            var result = _mockBaseItem.Object.IsVisibleViaTags(user, false);

            // Assert
            _mockLogger.Verify(
                x => x.LogDebug("{0} has an unrecognized parental rating of {1}.", "TestItem", "UnrecognizedRating"),
                Times.Once);
        }
    }
}
