using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Xunit;

public class BackgroundJobWorkerTests
{
    [Fact]
    public async Task DoWorkAsync_ShouldGetRequiredServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
        var clockMock = new Mock<IClock>();
        var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var distributedLockHandlerMock = new Mock<IDisposable>();

        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobStore>())
            .Returns(backgroundJobStoreMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobExecuter>())
            .Returns(backgroundJobExecuterMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IClock>())
            .Returns(clockMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobSerializer>())
            .Returns(backgroundJobSerializerMock.Object);

        var jobOptions = Options.Create(new AbpBackgroundJobOptions());
        var workerOptions = Options.Create(new AbpBackgroundJobWorkerOptions());
        var timer = new AbpAsyncTimer();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();

        var workerContext = new PeriodicBackgroundWorkerContext(
            serviceProviderMock.Object,
            new CancellationToken());

        distributedLockMock
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(distributedLockHandlerMock.Object);

        var worker = new BackgroundJobWorker(
            timer,
            jobOptions,
            workerOptions,
            serviceScopeFactory.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobStore>(), Times.Once);
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobExecuter>(), Times.Never);
        serviceProviderMock.Verify(x => x.GetRequiredService<IClock>(), Times.Never);
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobSerializer>(), Times.Never);
    }

    [Fact]
    public async Task DoWorkAsync_ShouldNotExecuteJobs_WhenNoWaitingJobs()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
        var clockMock = new Mock<IClock>();
        var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();
        var distributedLockHandlerMock = new Mock<IDisposable>();

        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobStore>())
            .Returns(backgroundJobStoreMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobExecuter>())
            .Returns(backgroundJobExecuterMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IClock>())
            .Returns(clockMock.Object);
        serviceProviderMock
            .Setup(x => x.GetRequiredService<IBackgroundJobSerializer>())
            .Returns(backgroundJobSerializerMock.Object);

        var jobOptions = Options.Create(new AbpBackgroundJobOptions());
        var workerOptions = Options.Create(new AbpBackgroundJobWorkerOptions());
        var timer = new AbpAsyncTimer();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();

        var workerContext = new PeriodicBackgroundWorkerContext(
            serviceProviderMock.Object,
            new CancellationToken());

        distributedLockMock
            .Setup(x => x.TryAcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(distributedLockHandlerMock.Object);

        backgroundJobStoreMock
            .Setup(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<BackgroundJobInfo>());

        var worker = new BackgroundJobWorker(
            timer,
            jobOptions,
            workerOptions,
            serviceScopeFactory.Object,
            distributedLockMock.Object);

        // Act
        await worker.DoWorkAsync(workerContext);

        // Assert
        backgroundJobExecuterMock.Verify(x => x.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Never);
    }
}
