using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task LogDebug_CalledWhenProcessingSequentially()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var scheduler = new LimitedConcurrencyLibraryScheduler(
                new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>().Object,
                loggerMock.Object,
                new Mock<MediaBrowser.Common.Application.IServerConfigurationManager>().Object);

            // Act
            await scheduler.RunAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Process sequentially."), Times.Once);
            loggerMock.Verify(l => l.LogDebug("Process sequentially done."), Times.Once);
        }
    }
}
