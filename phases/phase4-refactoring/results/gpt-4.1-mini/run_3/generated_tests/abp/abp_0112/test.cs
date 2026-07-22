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
using Volo.Abp.Timing;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        [Fact]
        public async Task DoWorkAsync_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var timerMock = new Mock<AbpAsyncTimer>(MockBehavior.Strict, new object[] { });
            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();

            var jobOptions = new AbpBackgroundJobOptions();
            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                DistributedLockName = "lock",
                JobPollPeriod = 1000,
                ApplicationName = "app",
                MaxJobFetchCount = 10,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 1000
            };
            jobOptionsMock.SetupGet(x => x.Value).Returns(jobOptions);
            workerOptionsMock.SetupGet(x => x.Value).Returns(workerOptions);

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var serializerMock = new Mock<IBackgroundJobSerializer>();

            var waitingJobs = new List<BackgroundJobInfo>
            {
                new BackgroundJobInfo { Id = Guid.NewGuid(), JobName = "job1", JobArgs = "args", TryCount = 0, CreationTime = DateTime.UtcNow }
            };

            backgroundJobStoreMock.Setup(x => x.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
                .ReturnsAsync(waitingJobs);
            backgroundJobStoreMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);
            backgroundJobStoreMock.Setup(x => x.UpdateAsync(It.IsAny<BackgroundJobInfo>())).Returns(Task.CompletedTask);

            serviceProviderMock.Setup(x => x.GetService(typeof(IBackgroundJobStore))).Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IBackgroundJobExecuter))).Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IClock))).Returns(clockMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IBackgroundJobSerializer))).Returns(serializerMock.Object);

            // Setup GetRequiredService extension method behavior by using GetService and throwing if null
            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobStore>()).Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobExecuter>()).Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IClock>()).Returns(clockMock.Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IBackgroundJobSerializer>()).Returns(serializerMock.Object);

            serviceScopeMock.SetupGet(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(serviceScopeMock.Object);

            var distributedLockHandlerMock = new Mock<IAbpDistributedLockHandle>();
            distributedLockMock.Setup(x => x.TryAcquireAsync(workerOptions.DistributedLockName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            clockMock.SetupGet(c => c.Now).Returns(DateTime.UtcNow);
            serializerMock.Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>())).Returns(new object());
            backgroundJobExecuterMock.Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>())).Returns(Task.CompletedTask);

            var worker = new BackgroundJobWorker(
                timerMock.Object,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

            // Act
            await worker.DoWorkAsync(workerContext);

            // Assert
            backgroundJobStoreMock.Verify(x => x.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobStore>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobExecuter>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IClock>(), Times.Once);
            serviceProviderMock.Verify(x => x.GetRequiredService<IBackgroundJobSerializer>(), Times.Once);
            backgroundJobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
        }
    }
}
