using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests
    {
        [Fact]
        public async Task Enqueue_ShouldLogDebug_WhenProcessSequentially()
        {
            // Arrange
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
            var loggerMock = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            // Mock the Configuration property to return a non-null ServerConfiguration object
            serverConfigurationManagerMock.Setup(x => x.Configuration).Returns(new ServerConfiguration { LibraryScanFanoutConcurrency = 1 });

            var scheduler = new LimitedConcurrencyLibraryScheduler(
                hostApplicationLifetimeMock.Object,
                loggerMock.Object,
                serverConfigurationManagerMock.Object);

            var data = new object[] { new object() };
            var worker = new Func<object, IProgress<double>, Task>((_, __) => Task.CompletedTask);
            var progress = new Progress<double>();
            var cancellationToken = CancellationToken.None;

            // Act
            await scheduler.Enqueue(data, worker, progress, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Process sequentially done.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
