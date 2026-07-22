using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_Should_Call_GetRequiredService_For_IBackgroundJobStore()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockStore = new Mock<IBackgroundJobStore>().Object;
        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(mockStore)
            .Verifiable();

        var workerContext = new PeriodicBackgroundWorkerContext(mockServiceProvider.Object);

        var mockTimer = new Mock<AbpAsyncTimer>().Object;
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>().Object;
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>().Object;
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>().Object;
        var mockDistributedLock = new Mock<IAbpDistributedLock>().Object;
        mockDistributedLock
            .Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IDistributedLockHandle>().Object);

        var worker = new TestableBackgroundJobWorker(
            mockTimer,
            mockJobOptions,
            mockWorkerOptions,
            mockServiceScopeFactory,
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }

    [Fact]
    public async Task DoWorkAsync_Should_Not_Call_GetRequiredService_When_Lock_Not_Acquired()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var workerContext = new PeriodicBackgroundWorkerContext(mockServiceProvider.Object);

        var mockTimer = new Mock<AbpAsyncTimer>().Object;
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>().Object;
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>().Object;
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>().Object;
        var mockDistributedLock = new Mock<IAbpDistributedLock>().Object;
        mockDistributedLock
            .Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IDistributedLockHandle)null);

        var worker = new TestableBackgroundJobWorker(
            mockTimer,
            mockJobOptions,
            mockWorkerOptions,
            mockServiceScopeFactory,
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Never);
    }
}

// Testable subclass to access protected DoWorkAsync
public class TestableBackgroundJobWorker : BackgroundJobWorker
{
    public TestableBackgroundJobWorker(
        AbpAsyncTimer timer,
        IOptions<AbpBackgroundJobOptions> jobOptions,
        IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
        IServiceScopeFactory serviceScopeFactory,
        IAbpDistributedLock distributedLock)
        : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
    {
    }

    public new async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await base.DoWorkAsync(workerContext);
    }
}
