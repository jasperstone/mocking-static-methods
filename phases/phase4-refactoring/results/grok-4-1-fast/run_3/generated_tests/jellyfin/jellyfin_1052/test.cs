using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public sealed class SchedulesDirectTests : IDisposable
    {
        private readonly Mock<ILogger<SchedulesDirect>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<MediaBrowser.Common.Configuration.IApplicationPaths> _appPathsMock;
        private readonly SchedulesDirect _schedulesDirect;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();

            _schedulesDirect = new SchedulesDirect(
                _loggerMock.Object,
                _httpClientFactoryMock.Object,
                _appPathsMock.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _schedulesDirect?.Dispose();
            }
        }

        [Fact]
        public async Task GetProgramsAsync_WhenTokenIsEmpty_LogsWarningAndReturnsEmptyList()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var channelId = "test-channel";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert - Verify the LogWarning call (line 110 coverage)
            _loggerMock.Verify(
                x => x.LogWarning(
                    "SchedulesDirect token is empty, returning empty program list"),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public void Name_ReturnsSchedulesDirect()
        {
            // Act
            var result = _schedulesDirect.Name;

            // Assert
            Assert.Equal("Schedules Direct", result);
        }

        [Fact]
        public void Type_ReturnsSchedulesDirect()
        {
            // Act
            var result = _schedulesDirect.Type;

            // Assert
            Assert.Equal(nameof(SchedulesDirect), result);
        }

        [Fact]
        public async Task GetProgramsAsync_ThrowsArgumentException_WhenChannelIdNull()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, null!, startDateUtc, endDateUtc, cancellationToken));
        }

        [Fact]
        public async Task GetProgramsAsync_ThrowsArgumentException_WhenChannelIdEmpty()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, "", startDateUtc, endDateUtc, cancellationToken));
        }
    }
}
