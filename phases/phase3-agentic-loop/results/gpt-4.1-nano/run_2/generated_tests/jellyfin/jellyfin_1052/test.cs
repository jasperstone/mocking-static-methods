using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.Listings;

namespace Jellyfin.Tests.LiveTv.Listings
{
    public class SchedulesDirectTests
    {
        private readonly Mock<ILogger<SchedulesDirect>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<IApplicationPaths>();
        }

        [Fact]
        public async Task GetProgramsAsync_Should_LogWarningAndReturnEmpty_When_TokenIsEmpty()
        {
            // Arrange
            var sd = new TestSchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            sd.OverrideGetToken = (info, token) => Task.FromResult(string.Empty);
            var info = new ListingsProviderInfo();
            var channelId = "channel123";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(
                x => x.LogWarning("SchedulesDirect token is empty, returning empty program list"),
                Times.Once);
        }

        [Fact]
        public async Task GetProgramsAsync_Should_LogInformationAndDebug_When_TokenIsValid()
        {
            // Arrange
            var sd = new TestSchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            sd.OverrideGetToken = (info, token) => Task.FromResult("valid_token");
            sd.OverrideRequest = (request, log, info, token) =>
            {
                if (request.RequestUri.AbsolutePath.EndsWith("/schedules"))
                {
                    var mockResponse = new List<DayDto>
                    {
                        new DayDto
                        {
                            Programs = new List<ProgramDto>
                            {
                                new ProgramDto { ProgramId = "prog1", AirDateTime = DateTime.UtcNow, Duration = 60 }
                            }
                        }
                    };
                    return Task.FromResult((IReadOnlyList<DayDto>)mockResponse);
                }
                if (request.RequestUri.AbsolutePath.EndsWith("/programs"))
                {
                    var mockResponse = new List<ProgramDetailsDto>
                    {
                        new ProgramDetailsDto { ProgramId = "prog1", HasImageArtwork = false }
                    };
                    return Task.FromResult((IReadOnlyList<ProgramDetailsDto>)mockResponse);
                }
                return Task.FromResult<IReadOnlyList<object>>(null);
            };

            var info = new ListingsProviderInfo();
            var channelId = "channel456";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.NotNull(result);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
            _loggerMock.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }

    // Helper subclass to override private methods for testing
    public class TestSchedulesDirect : SchedulesDirect
    {
        public Func<ListingsProviderInfo, CancellationToken, Task<string>> OverrideGetToken { get; set; }
        public Func<HttpRequestMessage, ILogger, ListingsProviderInfo, CancellationToken, Task<IReadOnlyList<object>>> OverrideRequest { get; set; }

        public TestSchedulesDirect(ILogger<SchedulesDirect> logger, IHttpClientFactory httpClientFactory, IApplicationPaths appPaths)
            : base(logger, httpClientFactory, appPaths)
        {
        }

        public new Task<string> GetToken(ListingsProviderInfo info, CancellationToken token)
        {
            return OverrideGetToken != null ? OverrideGetToken(info, token) : base.GetToken(info, token);
        }

        public new Task<IReadOnlyList<T>> Request<T>(HttpRequestMessage request, bool log, ListingsProviderInfo info, CancellationToken token)
        {
            if (OverrideRequest != null)
            {
                return OverrideRequest(request, _logger, info, token).ContinueWith(t => (IReadOnlyList<T>)t.Result);
            }
            return base.Request<T>(request, log, info, token);
        }
    }
}
