using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
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
            var jobInfo = new BackgroundJobInfo
            {
                Id = Guid.NewGuid(),
                JobName = "TestJob",
                JobArgs = "{}",
                TryCount = 0,
                CreationTime = DateTime.UtcNow.AddMinutes(-5)
            };

            var jobList = new List<BackgroundJobInfo> { jobInfo };

            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedLockHandlerMock = new Mock<IAbpDistributedLockHandle>();

            var jobOptions = new AbpBackgroundJobOptions();
            jobOptions.Jobs["TestJob"] = new BackgroundJobOptions.JobInfo
            {
                JobType = typeof(object),
                ArgsType = typeof(object)
            };

            jobOptionsMock.Setup(j => j.Value).Returns(jobOptions);

            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DistributedLockName = "lock",
                JobPollPeriod = 1000,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 1000
            };
            workerOptionsMock.Setup(w => w.Value).Returns(workerOptions);

            distributedLockMock.Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            var timer = new AbpAsyncTimer();

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            backgroundJobStoreMock.Setup(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
                .ReturnsAsync(jobList);

            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            backgroundJobExecuterMock.Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()))
                .Returns(Task.CompletedTask);

            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

            var serializerMock = new Mock<IBackgroundJobSerializer>();
            serializerMock.Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>())).Returns(new object());

            // Setup GetRequiredService calls on serviceProviderMock
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobStore)))
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobExecuter)))
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IClock)))
                .Returns(clockMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IBackgroundJobSerializer)))
                .Returns(serializerMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

            var worker = new BackgroundJobWorker(
                timer,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            await worker.InvokeDoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobStore)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobExecuter)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IClock)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobSerializer)), Times.AtLeastOnce);

            backgroundJobStoreMock.Verify(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount), Times.Once);
            backgroundJobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
        }
    }

    // Extension to invoke protected DoWorkAsync for testing
    internal static class BackgroundJobWorkerTestExtensions
    {
        public static Task InvokeDoWorkAsync(this BackgroundJobWorker worker, PeriodicBackgroundWorkerContext context)
        {
            return worker.InvokeProtectedDoWorkAsync(context);
        }

        private static Task InvokeProtectedDoWorkAsync(this BackgroundJobWorker worker, PeriodicBackgroundWorkerContext context)
        {
            var method = typeof(BackgroundJobWorker).GetMethod("DoWorkAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Task)method.Invoke(worker, new object[] { context });
        }
    }
}
