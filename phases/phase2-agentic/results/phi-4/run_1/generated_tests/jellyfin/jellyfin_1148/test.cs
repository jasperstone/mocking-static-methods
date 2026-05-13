using System;
using System.IO;
using System.Linq;
using Jellyfin.LiveTv.TunerHosts;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TunerHostManagerTests
{
    [Fact]
    public void DeleteTunerHost_LogsWarning_WhenIOExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TunerHostManager>>();
        var configMock = new Mock<IConfigurationManager>();
        var taskManagerMock = new Mock<ITaskManager>();
        var tunerHosts = Array.Empty<ITunerHost>();

        var tunerHostManager = new TunerHostManager(
            loggerMock.Object,
            configMock.Object,
            taskManagerMock.Object,
            tunerHosts);

        var tunerId = Guid.NewGuid().ToString("N");
        var channelCacheFile = Path.Combine("cachePath", tunerId + "_channels");

        // Ensure the file exists to simulate an IOException on deletion
        File.WriteAllText(channelCacheFile, string.Empty);

        // Act
        tunerHostManager.DeleteTunerHost(tunerId);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((ex, state) => ex is IOException),
                It.Is<Exception>(ex => ex is IOException),
                It.Is<Func<It.IsAnyType, Exception, string>>((state, ex) => state.ToString().Contains(tunerId))
            ),
            Times.Once
        );

        // Clean up
        File.Delete(channelCacheFile);
    }
}
