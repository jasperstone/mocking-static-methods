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
            var timer = new AbpAsyncTimerFake();
            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();

            var jobOptions = new AbpBackgroundJobOptions();
            jobOptionsMock.Setup(j => j.Value).Returns(jobOptions);

            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DistributedLockName = "TestLock",
                JobPollPeriod = 1000,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 2,
                DefaultTimeout = 60
            };
            workerOptionsMock.Setup(w => w.Value).Returns(workerOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            backgroundJobStoreMock.Setup(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
                .ReturnsAsync(new List<BackgroundJobInfo>
                {
                    new BackgroundJobInfo
                    {
                        Id = Guid.NewGuid(),
                        JobName = "TestJob",
                        JobArgs = "{}",
                        TryCount = 0,
                        CreationTime = DateTime.UtcNow
                    }
                });

            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            backgroundJobExecuterMock.Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>())).Returns(Task.CompletedTask);

            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTime.UtcNow);

            var serializerMock = new Mock<IBackgroundJobSerializer>();
            serializerMock.Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>())).Returns(new object());

            // Setup serviceProvider to return mocks for required services
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobStore>()).Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>()).Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IClock>()).Returns(clockMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>()).Returns(serializerMock.Object);

            var cancellationTokenSource = new CancellationTokenSource();

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, cancellationTokenSource.Token);

            var distributedLockHandlerMock = new Mock<IAbpDistributedLockHandle>();
            distributedLockMock.Setup(dl => dl.TryAcquireAsync(workerOptions.DistributedLockName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            var worker = new BackgroundJobWorker(
                timer,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            var doWorkAsyncMethod = typeof(BackgroundJobWorker).GetMethod("DoWorkAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)doWorkAsyncMethod.Invoke(worker, new object[] { workerContext });
            await task;

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.AtLeastOnce);
        }

        // Minimal fake for AbpAsyncTimer to satisfy constructor dependency
        private class AbpAsyncTimerFake : AbpAsyncTimer
        {
            public override int Period { get; set; }
        }
    }
}
