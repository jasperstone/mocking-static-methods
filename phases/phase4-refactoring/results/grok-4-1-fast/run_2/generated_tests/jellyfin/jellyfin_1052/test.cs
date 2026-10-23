using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
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
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly SchedulesDirect _schedulesDirect;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<IApplicationPaths> { DefaultValue = DefaultValue.Mock };

            _appPathsMock.Setup(x => x.CachePath).Returns("/tmp");

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
        public async Task GetProgramsAsync_WithEmptyToken_LogsWarningAndReturnsEmptyList()
        {
            // Arrange
            var info = new ListingsProviderInfo { Id = "test-provider" };
            var channelId = "test-channel";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // In test environment without real tokens, GetToken returns empty, triggering the warning path

            // Act
            var result = await _schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SchedulesDirect token is empty")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProgramsAsync_NullChannelId_ThrowsArgumentException()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, null!, startDateUtc, endDateUtc, cancellationToken));
            
            Assert.Contains("channelId", exception.ParamName ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetProgramsAsync_EmptyChannelId_ThrowsArgumentException()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _schedulesDirect.GetProgramsAsync(info, string.Empty, startDateUtc, endDateUtc, cancellationToken));
            
            Assert.Contains("channelId", exception.ParamName ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}
