using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Volo.Abp.BackgroundJobs;
using Xunit;

namespace Volo.Abp.BackgroundJobs.Tests
{
    public class BackgroundJobWorkerTests
    {
        private class TestBackgroundJobWorker : BackgroundJobWorker
        {
            public TestBackgroundJobWorker(
                AbpAsyncTimer timer,
                IOptions<AbpBackgroundJobOptions> jobOptions,
                IOptions<AbpBackgroundJobWorkerOptions> workerOptions,
                IServiceScopeFactory serviceScopeFactory,
                IAbpDistributedLock distributedLock)
                : base(timer, jobOptions, workerOptions, serviceScopeFactory, distributedLock)
            {
            }

            public async Task InvokeDoWorkAsync(PeriodicBackgroundWorkerContext context)
            {
                await DoWorkAsync(context);
            }
        }

        [Fact]
        public async Task DoWorkAsync_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var jobInfo = new BackgroundJobInfo
            {
                JobName = "TestJob",
                JobArgs = "{}",
                TryCount = 0
            };

            var jobList = new List<BackgroundJobInfo> { jobInfo };

            var jobOptionsMock = new Mock<IOptions<AbpBackgroundJobOptions>>();
            var workerOptionsMock = new Mock<IOptions<AbpBackgroundJobWorkerOptions>>();
            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedLockHandlerMock = new Mock<IAbpDistributedLockHandle>();

            var jobOptions = new AbpBackgroundJobOptions();
            jobOptions.Jobs.Add(new JobInfo("TestJob", typeof(object), typeof(object)));
            jobOptionsMock.Setup(j => j.Value).Returns(jobOptions);

            var workerOptions = new AbpBackgroundJobWorkerOptions
            {
                ApplicationName = "TestApp",
                MaxJobFetchCount = 10,
                DistributedLockName = "lock",
                JobPollPeriod = 1000
            };
            workerOptionsMock.Setup(w => w.Value).Returns(workerOptions);

            distributedLockMock
                .Setup(d => d.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            backgroundJobStoreMock
                .Setup(s => s.GetWaitingJobsAsync(workerOptions.ApplicationName, workerOptions.MaxJobFetchCount))
                .ReturnsAsync(jobList);

            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            backgroundJobExecuterMock
                .Setup(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()))
                .Returns(Task.CompletedTask);

            var clockMock = new Mock<IClock>();
            clockMock.Setup(c => c.Now).Returns(DateTimeOffset.UtcNow);

            var serializerMock = new Mock<IBackgroundJobSerializer>();
            serializerMock
                .Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>()))
                .Returns(new object());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IBackgroundJobStore)))
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IBackgroundJobExecuter)))
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IClock)))
                .Returns(clockMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IBackgroundJobSerializer)))
                .Returns(serializerMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock
                .Setup(f => f.CreateScope())
                .Returns(serviceScopeMock.Object);

            var timer = new AbpAsyncTimer();

            var worker = new TestBackgroundJobWorker(
                timer,
                jobOptionsMock.Object,
                workerOptionsMock.Object,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

            // Act
            await worker.InvokeDoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobStore)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobExecuter)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IClock)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IBackgroundJobSerializer)), Times.Once);
            backgroundJobExecuterMock.Verify(e => e.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
        }
    }
}
