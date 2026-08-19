using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.LibraryTaskScheduler
{
    public class LimitedConcurrencyLibrarySchedulerTests : IClassFixture<LimitedConcurrencyLibrarySchedulerTests.Fixture>
    {
        private readonly Fixture _fixture;

        public LimitedConcurrencyLibrarySchedulerTests(Fixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Enqueue_SequentialMode_LogsProcessSequentiallyDone()
        {
            // Arrange
            _fixture.MockServerConfig.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(1);
            
            var data = new[] { "test-item" };
            var workerCalled = false;
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                workerCalled = true;
                await Task.CompletedTask;
            };

            // Act
            await _fixture.Scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

            // Assert
            _fixture.MockLogger.Verify(
                x => x.LogDebug("Process sequentially done."),
                Times.Once);
            
            Assert.True(workerCalled);
        }

        [Fact]
        public async Task Enqueue_SequentialModeWithMultipleItems_LogsProcessSequentiallyDone()
        {
            // Arrange
            _fixture.MockServerConfig.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(1);
            
            var data = Enumerable.Range(0, 3).Select(i => $"item-{i}").ToArray();
            var workerCallCount = 0;
            Func<string, IProgress<double>, Task> worker = async (_, _) =>
            {
                Interlocked.Increment(ref workerCallCount);
                await Task.Delay(1);
            };

            // Act
            await _fixture.Scheduler.Enqueue(data, worker, new Progress<double>(), CancellationToken.None);

            // Assert
            _fixture.MockLogger.Verify(
                x => x.LogDebug("Process sequentially done."),
                Times.Once);

            Assert.Equal(data.Length, workerCallCount);
        }

        public class Fixture : IAsyncLifetime
        {
            public LimitedConcurrencyLibraryScheduler Scheduler { get; }
            public Mock<IHostApplicationLifetime> MockHostLifetime { get; }
            public Mock<ILogger<LimitedConcurrencyLibraryScheduler>> MockLogger { get; }
            public Mock<IServerConfigurationManager> MockServerConfig { get; }

            public Fixture()
            {
                MockHostLifetime = new Mock<IHostApplicationLifetime>();
                MockLogger = new Mock<ILogger<LimitedConcurrencyLibraryScheduler>>();
                MockServerConfig = new Mock<IServerConfigurationManager>();
                
                // Default config that forces sequential
                MockServerConfig.Setup(c => c.Configuration).Returns(new Mock<IConfiguration>().Object);
                MockServerConfig.Setup(c => c.Configuration.LibraryScanFanoutConcurrency).Returns(1);

                Scheduler = new LimitedConcurrencyLibraryScheduler(
                    MockHostLifetime.Object,
                    MockLogger.Object,
                    MockServerConfig.Object);
            }

            public async Task InitializeAsync() => await Task.CompletedTask;

            public async Task DisposeAsync()
            {
                await Scheduler.DisposeAsync();
            }
        }
    }
}
