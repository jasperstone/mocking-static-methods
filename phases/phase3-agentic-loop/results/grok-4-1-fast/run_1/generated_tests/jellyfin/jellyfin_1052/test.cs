using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.LiveTv.Tests.Listings
{
    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_EmptyToken_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var appPathsMock = new Mock<IApplicationPaths>();
            
            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            // Mock GetToken to return empty string by setting private _tokens field to empty
            var tokensField = typeof(SchedulesDirect).GetField("_tokens", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tokensField?.SetValue(schedulesDirect, new Dictionary<string, object>());

            var info = new ListingsProviderInfo { Name = "Test" };
            var channelId = "testChannel";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProgramsAsync_NonEmptyToken_DoesNotLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var appPathsMock = new Mock<IApplicationPaths>();
            
            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            // Set up non-empty token scenario (mock GetToken to not return empty)
            var tokensField = typeof(SchedulesDirect).GetField("_tokens", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tokens = new Dictionary<string, object>();
            tokens["test"] = new KeyValuePair<string, string>("key", "validtoken");
            tokensField?.SetValue(schedulesDirect, tokens);

            var info = new ListingsProviderInfo { Name = "Test" };
            var channelId = "testChannel";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert - warning should not be called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
