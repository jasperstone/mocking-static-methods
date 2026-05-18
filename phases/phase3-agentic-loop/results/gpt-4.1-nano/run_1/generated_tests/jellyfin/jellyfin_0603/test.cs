using System;
using System.Collections.Generic;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Tests
{
    public class BaseItemTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> LogMessages { get; } = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LogMessages.Add(formatter(state, exception));
            }
        }

        private class TestItem : BaseItem
        {
            public ILogger Logger { get; set; }
            public override string Name { get => base.Name; set => base.Name = value; }
            public override string Path { get => base.Path; set => base.Path = value; }
            public override Guid Id { get => base.Id; set => base.Id = value; }
            public override string OfficialRatingForComparison { get; set; }
            public override string CustomRatingForComparison { get; set; }
            public override bool SupportsPlayedStatus => true;
            public override bool SupportsPositionTicksResume => true;
            public override bool SupportsAddingToPlaylist => true;
            public override bool AlwaysScanInternalMetadataPath => true;

            public override string GetPreferredMetadataCountryCode() => "US";

            public override float? GetRatingScore(string rating, string countryCode)
            {
                if (rating == "Unrecognized")
                {
                    return null;
                }
                return new ParentalRatingScore { Score = 5, SubScore = 2 };
            }

            public override bool IsVisibleViaTags(object user, bool skipAllowedTagsCheck) => true;
        }

        [Fact]
        public void LogDebug_IsCalled_WhenRatingScoreIsNull()
        {
            // Arrange
            var logger = new DummyLogger();
            var item = new TestItem
            {
                Logger = logger,
                Name = "TestItem",
                OfficialRatingForComparison = "Unrecognized",
                CustomRatingForComparison = null
            };
            var user = new object();

            // Act
            var result = item.IsVisibleViaRating(user, skipAllowedTagsCheck: false);

            // Assert
            Assert.Contains($"{item.Name} has an unrecognized parental rating of {item.CustomRatingForComparison}.", logger.LogMessages);
            Assert.False(result);
        }
    }
}
