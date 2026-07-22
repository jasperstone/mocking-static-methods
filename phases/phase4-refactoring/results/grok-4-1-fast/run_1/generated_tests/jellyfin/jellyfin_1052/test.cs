using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests : IDisposable
    {
        private readonly Mock<ILogger<SchedulesDirect>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<MediaBrowser.Common.Configuration.IApplicationPaths> _appPathsMock;
        private readonly TestSchedulesDirect _testSubject;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();

            _testSubject = new TestSchedulesDirect(
                _loggerMock.Object,
                _httpClientFactoryMock.Object,
                _appPathsMock.Object);
        }

        public void Dispose()
        {
            _testSubject.Dispose();
        }

        [Fact]
        public async Task GetProgramsAsync_WhenTokenIsEmpty_LogsWarningAndReturnsEmptyList()
        {
            // Arrange
            var info = new ListingsProviderInfo { Name = "Test" };
            var channelId = "EP00012345";
            var startDateUtc = DateTime.UtcNow.AddDays(-1);
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            _testSubject.SetGetTokenResult(string.Empty);

            // Act
            var result = await _testSubject.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "SchedulesDirect token is empty, returning empty program list"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Empty(result);
        }

        [Fact]
        public void GetScheduleRequestDates_ReturnsExpectedDates()
        {
            // Arrange
            var startDateUtc = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var endDateUtc = new DateTime(2023, 1, 3, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var dates = SchedulesDirect.GetScheduleRequestDates(startDateUtc, endDateUtc);

            // Assert
            var expectedDates = new[] { "2023-01-01", "2023-01-02", "2023-01-03" };
            Assert.Equal(expectedDates, dates);
        }
    }

    public class TestSchedulesDirect : SchedulesDirect
    {
        private Func<ListingsProviderInfo, CancellationToken, Task<string>> _getTokenDelegate = 
            (info, ct) => Task.FromResult((string)null);

        public TestSchedulesDirect(
            ILogger<SchedulesDirect> logger, 
            IHttpClientFactory httpClientFactory, 
            MediaBrowser.Common.Configuration.IApplicationPaths appPaths)
            : base(logger, httpClientFactory, appPaths)
        {
        }

        public void SetGetTokenResult(string token)
        {
            _getTokenDelegate = (info, ct) => Task.FromResult(token);
        }

        public override async Task<string> GetToken(ListingsProviderInfo info, CancellationToken cancellationToken)
        {
            return await _getTokenDelegate(info, cancellationToken).ConfigureAwait(false);
        }
    }
}
