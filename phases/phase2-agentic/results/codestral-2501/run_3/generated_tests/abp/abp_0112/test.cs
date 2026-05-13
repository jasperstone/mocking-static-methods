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

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var jobOptions = new AbpBackgroundJobOptions();
        var workerOptions = new AbpBackgroundJobWorkerOptions();

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var backgroundJobWorker = new BackgroundJobWorker(
            timerMock.Object,
            Options.Create(jobOptions),
            Options.Create(workerOptions),
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        // Act
        await backgroundJobWorker.DoWorkAsync(workerContextMock.Object);

        // Assert
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobStore>(), Times.Once);
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobExecuter>(), Times.Never);
        serviceProviderMock.Verify(x => x.GetRequiredService<IClock>(), Times.Never);
        serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobSerializer>(), Times.Never);
    }

    [Fact]
    public async Task DoWorkAsync_ShouldNotExecuteJobsIfNoWaitingJobs()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
        var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
        var clockMock = new Mock<IClock>();
        var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
        var distributedLockMock = new Mock<IAbpDistributedLock>();

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

        var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
        workerContextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        var jobOptions = new AbpBackgroundJobOptions();
        var workerOptions = new AbpBackgroundJobWorkerOptions();

        var timerMock = new Mock<AbpAsyncTimer>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        var backgroundJobWorker = new BackgroundJobWorker(
            timerMock.Object,
            Options.Create(jobOptions),
            Options.Create(workerOptions),
            serviceScopeFactoryMock.Object,
            distributedLockMock.Object);

        backgroundJobStoreMock
            .Setup(x => x.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
            .ReturnsAsync(new List<BackgroundJobInfo>());

        // Act
        await backgroundJobWorker.DoWorkAsync(workerContextMock.Object);

        // Assert
        backgroundJobExecuterMock.Verify(x => x.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Never);
    }
}
