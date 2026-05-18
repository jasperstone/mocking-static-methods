using System;
using System.Collections.Generic;
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
    public async Task Should_Call_GetRequiredService_On_WorkerContext_ServiceProvider()
    {
        // Arrange
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        var workerOptions = new AbpBackgroundJobWorkerOptions
        {
            JobPollPeriod = 1000,
            DistributedLockName = "test-lock",
            ApplicationName = "test-app",
            MaxJobFetchCount = 10
        };
        mockWorkerOptions.Setup(o => o.Value).Returns(workerOptions);

        var jobOptions = new AbpBackgroundJobOptions();
        mockJobOptions.Setup(o => o.Value).Returns(jobOptions);

        var serviceProviderMock = new Mock<IServiceProvider>();
        var storeMock = new Mock<IBackgroundJobStore>();
        storeMock.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
                 .ReturnsAsync(new List<BackgroundJobInfo>());
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
                          .Returns(storeMock.Object);

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.SetupGet(c => c.ServiceProvider).Returns(serviceProviderMock.Object);
        workerContextMock.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);

        var lockHandlerMock = new Mock<IAsyncDisposable>();
        mockDistributedLock.Setup(l => l.TryAcquireAsync("test-lock", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(lockHandlerMock.Object);

        var worker = new BackgroundJobWorkerTestWrapper(
            mockTimer.Object,
            mockJobOptions.Object,
            mockWorkerOptions.Object,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsyncProtected(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
        storeMock.Verify(s => s.GetWaitingJobsAsync("test-app", 10), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Call_GetRequiredService_When_Lock_Not_Acquired()
    {
        // Arrange
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        var workerOptions = new AbpBackgroundJobWorkerOptions
        {
            JobPollPeriod = 1000,
            DistributedLockName = "test-lock",
            ApplicationName = "test-app",
            MaxJobFetchCount = 10
        };
        mockWorkerOptions.Setup(o => o.Value).Returns(workerOptions);

        var serviceProviderMock = new Mock<IServiceProvider>();
        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.SetupGet(c => c.ServiceProvider).Returns(serviceProviderMock.Object);

        mockDistributedLock.Setup(l => l.TryAcquireAsync("test-lock", It.IsAny<CancellationToken>()))
                          .ReturnsAsync((IAsyncDisposable)null);

        var worker = new BackgroundJobWorkerTestWrapper(
            mockTimer.Object,
            mockJobOptions.Object,
            mockWorkerOptions.Object,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsyncProtected(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Never);
    }
}

// Test wrapper to access protected method
public class BackgroundJobWorkerTestWrapper : BackgroundJobWorker
{
    public BackgroundJobWorkerTestWrapper(
        AbpAsyncTimer timer,
        IOptions<AbpBackgroundJobOptions> jobOptions,
        IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
        IServiceScopeFactory serviceScopeFactory,
        IAbpDistributedLock distributedLock)
        : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
    {
    }

    public Task DoWorkAsyncProtected(PeriodicBackgroundWorkerContext workerContext)
    {
        return DoWorkAsync(workerContext);
    }
}
