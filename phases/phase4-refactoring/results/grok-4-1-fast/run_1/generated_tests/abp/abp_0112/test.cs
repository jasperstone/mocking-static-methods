using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task Should_Call_GetRequiredService_For_IBackgroundJobStore()
    {
        // Arrange
        var mockTimer = new Mock<AbpAsyncTimer>();
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        mockJobOptions.Setup(o => o.Value).Returns(new AbpBackgroundJobOptions());
        mockWorkerOptions.Setup(o => o.Value)
            .Returns(new AbpBackgroundJobWorkerOptions
            {
                JobPollPeriod = 1000,
                DistributedLockName = "test-lock",
                ApplicationName = "test-app",
                MaxJobFetchCount = 10
            });

        var workerContextServiceProvider = new Mock<IServiceProvider>();
        var mockStore = new Mock<IBackgroundJobStore>();
        workerContextServiceProvider
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(mockStore.Object)
            .Verifiable();

        mockStore.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<BackgroundJobInfo>());

        var workerContext = new PeriodicBackgroundWorkerContext(workerContextServiceProvider.Object);

        mockDistributedLock
            .Setup(l => l.TryAcquireAsync("test-lock", default))
            .ReturnsAsync((IDistributedLockHandle)null);

        var worker = new BackgroundJobWorkerTest(
            mockTimer.Object,
            mockJobOptions.Object,
            mockWorkerOptions.Object,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        // Act
        await worker.DoWorkAsyncProtected(workerContext);

        // Assert
        workerContextServiceProvider.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Set_Timer_Period_From_WorkerOptions()
    {
        // Arrange
        var mockTimer = new Mock<AbpAsyncTimer>();
        mockTimer.SetupProperty(t => t.Period);
        var mockJobOptions = new Mock<IOptions<AbpBackgroundJobOptions>>();
        var mockWorkerOptions = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockDistributedLock = new Mock<IAbpDistributedLock>();

        var expectedPeriod = 30000;
        mockWorkerOptions.Setup(o => o.Value).Returns(new AbpBackgroundJobWorkerOptions
        {
            JobPollPeriod = expectedPeriod
        });

        // Act
        var worker = new BackgroundJobWorkerTest(
            mockTimer.Object,
            mockJobOptions.Object,
            mockWorkerOptions.Object,
            mockServiceScopeFactory.Object,
            mockDistributedLock.Object);

        // Assert
        Assert.Equal(expectedPeriod, mockTimer.Object.Period);
    }
}

// Test-specific subclass to access protected method
public class BackgroundJobWorkerTest : BackgroundJobWorker
{
    public BackgroundJobWorkerTest(
        AbpAsyncTimer timer,
        IOptions<AbpBackgroundJobOptions> jobOptions,
        IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
        IServiceScopeFactory serviceScopeFactory,
        IAbpDistributedLock distributedLock)
        : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
    {
    }

    public async Task DoWorkAsyncProtected(PeriodicBackgroundWorkerContext workerContext)
    {
        await DoWorkAsync(workerContext);
    }

    public new AbpAsyncTimer Timer => base.Timer;
}
