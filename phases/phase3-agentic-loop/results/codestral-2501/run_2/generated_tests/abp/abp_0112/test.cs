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
            var timerMock = new Mock<AbpAsyncTimer>();
            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var workerContextMock = new Mock<PeriodicBackgroundWorkerContext>();
            workerContextMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobStore>()).Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobExecuter>()).Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IClock>()).Returns(clockMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobSerializer>()).Returns(backgroundJobSerializerMock.Object);

            var jobInfo = new BackgroundJobInfo { JobName = "TestJob", JobArgs = "TestArgs" };
            backgroundJobStoreMock.Setup(x => x.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new List<BackgroundJobInfo> { jobInfo });

            var worker = new BackgroundJobWorker(
                timerMock.Object,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            await worker.DoWorkAsync(workerContextMock.Object);

            // Assert
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobStore>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobExecuter>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IClock>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobSerializer>(), Times.Once);
        }
    }
}
