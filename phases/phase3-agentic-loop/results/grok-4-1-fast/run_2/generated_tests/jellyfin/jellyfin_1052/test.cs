using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.Listings
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
        public async Task GetProgramsAsync_EmptyToken_LogsWarningAndReturnsEmptyList()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var channelId = "TEST123";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Use reflection to set up GetToken to return empty string
            var getTokenField = typeof(SchedulesDirect).GetField("_tokens", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            getTokenField.SetValue(_schedulesDirect, new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Specialized.NameValuePair>());

            // Act
            var result = await _schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning("SchedulesDirect token is empty, returning empty program list"),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProgramsAsync_NullChannelId_ThrowsArgumentException()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, null!, DateTime.UtcNow, DateTime.UtcNow, cancellationToken));
        }

        [Fact]
        public async Task GetProgramsAsync_EmptyChannelId_ThrowsArgumentException()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, string.Empty, DateTime.UtcNow, DateTime.UtcNow, cancellationToken));
        }
    }
}
