using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldCallGetRequiredServiceForBackgroundJobStore_WhenLockIsAcquired()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockBackgroundJobStore = new Mock<IBackgroundJobStore>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = Options.Create(new AbpBackgroundJobOptions());
        var mockWorkerOptions = Options.Create(new AbpBackgroundJobWorkerOptions
        {
            ApplicationName = "TestApp",
            MaxJobFetchCount = 10,
            DistributedLockName = "TestLock",
            JobPollPeriod = TimeSpan.FromSeconds(1)
        });

        mockServiceProvider.Setup(s => s.GetRequiredService<IBackgroundJobStore>()).Returns(mockBackgroundJobStore.Object);

        var workerContext = new PeriodicBackgroundWorkerContext(mockServiceProvider.Object, CancellationToken.None);
        var worker = new BackgroundJobWorker(
            mockTimer.Object,
            mockJobOptions,
            mockWorkerOptions,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        mockDistributedLock.Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AbpDistributedLockHandler());

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        mockServiceProvider.Verify(s => s.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }
}
