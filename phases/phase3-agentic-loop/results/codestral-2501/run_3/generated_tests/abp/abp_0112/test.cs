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

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_ShouldCallGetRequiredService_WhenWaitingJobsExist()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();

            var jobOptions = new AbpBackgroundJobOptions();
            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DistributedLockName = "TestLock"
            };

            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            jobOptionsMock.Setup(o => o.Value).Returns(jobOptions);

            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            workerOptionsMock.Setup(o => o.Value).Returns(workerOptions);

            var timerMock = new Mock<AbpAsyncTimer>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var workerContext = new PeriodicBackgroundWorkerContext(
                serviceProviderMock.Object,
                CancellationToken.None
            );

            var waitingJobs = new List<BackgroundJobInfo>
            {
                new BackgroundJobInfo { JobName = "TestJob", JobArgs = "TestArgs" }
            };

            backgroundJobStoreMock.Setup(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
                .ReturnsAsync(waitingJobs);

            serviceProviderMock.Setup(s => s.GetRequiredService<IBackgroundJobStore>())
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IBackgroundJobExecuter>())
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IClock>())
                .Returns(clockMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IBackgroundJobSerializer>())
                .Returns(backgroundJobSerializerMock.Object);

            distributedLockMock.Setup(d => d.TryAcquireAsync(workerOptions.DistributedLockName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<IDisposable>().Object);

            var backgroundJobWorker = new BackgroundJobWorker(
                timerMock.Object,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IBackgroundJobStore>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IBackgroundJobExecuter>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IClock>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IBackgroundJobSerializer>(), Times.Once);
        }
    }
}
