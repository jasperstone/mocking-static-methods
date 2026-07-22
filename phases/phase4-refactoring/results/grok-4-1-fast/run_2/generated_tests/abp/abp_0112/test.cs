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

    public async Task DoWorkAsyncProtected(PeriodicBackgroundWorkerContext workerContext)
    {
        await DoWorkAsync(workerContext);
    }
}

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_Should_Call_GetRequiredService_For_IBackgroundJobStore_When_Lock_Acquired()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(backgroundJobStoreMock.Object)
            .Verifiable();

        var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

        var distributedLockHandlerMock = new Mock<IAsyncDisposable>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        distributedLockMock
            .Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(distributedLockHandlerMock.Object);

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        jobOptionsMock.Setup(o => o.Value).Returns(new AbpBackgroundJobOptions());

        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        workerOptionsMock.Setup(o => o.Value)
            .Returns(new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                JobPollPeriod = TimeSpan.FromSeconds(10),
                DistributedLockName = "TestLock"
            });

        var worker = new TestableBackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsyncProtected(workerContext);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }

    [Fact]
    public async Task DoWorkAsync_Should_Not_Call_GetRequiredService_When_Lock_Not_Acquired()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Throws(new InvalidOperationException("Should not be called"));

        var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

        var distributedLockMock = new Mock<IAbpDistributedLock>();
        distributedLockMock
            .Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IAsyncDisposable?)null);

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        jobOptionsMock.Setup(o => o.Value).Returns(new AbpBackgroundJobOptions());

        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        workerOptionsMock.Setup(o => o.Value)
            .Returns(new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                JobPollPeriod = TimeSpan.FromSeconds(10),
                DistributedLockName = "TestLock"
            });

        var worker = new TestableBackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act & Assert
        await worker.DoWorkAsyncProtected(workerContext);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Never);
    }
}
