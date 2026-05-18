using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task Should_Call_GetRequiredService_For_IBackgroundJobStore_When_Lock_Acquired()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var storeMock = new Mock<IBackgroundJobStore>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
            .Returns(storeMock.Object)
            .Verifiable();

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.SetupGet(wc => wc.ServiceProvider).Returns(serviceProviderMock.Object);
        workerContextMock.SetupGet(wc => wc.CancellationToken).Returns(CancellationToken.None);

        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var lockHandlerMock = new Mock<IAsyncDisposable>();
        distributedLockMock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockHandlerMock.Object);

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var jobOptions = new AbpBackgroundJobOptions();
        var workerOptions = new AbpBackgroundJobWorkerOptions
        {
            JobPollPeriod = TimeSpan.FromSeconds(1),
            DistributedLockName = "test-lock",
            ApplicationName = "test-app",
            MaxJobFetchCount = 10
        };

        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        jobOptionsMock.Setup(o => o.Value).Returns(jobOptions);

        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        workerOptionsMock.Setup(o => o.Value).Returns(workerOptions);

        var worker = new TestableBackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsync(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Call_GetRequiredService_For_IBackgroundJobStore_When_Lock_Not_Acquired()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.SetupGet(wc => wc.ServiceProvider).Returns(serviceProviderMock.Object);
        workerContextMock.SetupGet(wc => wc.CancellationToken).Returns(CancellationToken.None);

        var distributedLockMock = new Mock<IAbpDistributedLock>();
        distributedLockMock
            .Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IAsyncDisposable)null);

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var jobOptions = new AbpBackgroundJobOptions();
        var workerOptions = new AbpBackgroundJobWorkerOptions
        {
            JobPollPeriod = TimeSpan.FromSeconds(1),
            DistributedLockName = "test-lock",
            ApplicationName = "test-app",
            MaxJobFetchCount = 10
        };

        var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
        jobOptionsMock.Setup(o => o.Value).Returns(jobOptions);

        var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
        workerOptionsMock.Setup(o => o.Value).Returns(workerOptions);

        var worker = new TestableBackgroundJobWorker(
            timerMock.Object,
            jobOptionsMock.Object,
            workerOptionsMock.Object,
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsync(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Never);
    }
}

// Testable version with protected method exposed
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
