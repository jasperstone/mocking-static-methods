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
            var jobOptions = Options.Create(new AbpBackgroundJobOptions());
            var workerOptions = Options.Create(new AbpBackgroundJobWorkerOptions
            {
                DistributedLockName = "lock",
                ApplicationName = "app",
                MaxJobFetchCount = 10,
                JobPollPeriod = 1000,
                DefaultFirstWaitDuration = 1,
                DefaultWaitFactor = 1,
                DefaultTimeout = 1000
            });

            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedLockHandlerMock = new Mock<IAsyncDisposable>();
            distributedLockMock.Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var serializerMock = new Mock<IBackgroundJobSerializer>();

            var now = DateTime.UtcNow;
            clockMock.Setup(c => c.Now).Returns(now);

            var jobInfo = new BackgroundJobInfo
            {
                Id = Guid.NewGuid(),
                JobName = "TestJob",
                JobArgs = "{}",
                TryCount = 0,
                CreationTime = now.AddMinutes(-1)
            };

            backgroundJobStoreMock.Setup(s => s.GetWaitingJobsAsync(workerOptions.Value.ApplicationName, workerOptions.Value.MaxJobFetchCount))
                .ReturnsAsync(new List<BackgroundJobInfo> { jobInfo });

            serializerMock.Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>())).Returns(new object());

            var jobConfiguration = new BackgroundJobOptions.JobConfiguration(typeof(object), typeof(object));
            jobOptions.Value.Jobs["TestJob"] = jobConfiguration;

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobStore))).Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobExecuter))).Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IClock))).Returns(clockMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobSerializer))).Returns(serializerMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var timer = new AbpAsyncTimer();

            var worker = new BackgroundJobWorker(
                timer,
                jobOptions,
                workerOptions,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

            // Act
            await worker.InvokeDoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobExecuter>(), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IClock>(), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobSerializer>(), Times.AtLeastOnce);
            backgroundJobExecuterMock.Verify(executer => executer.ExecuteAsync(It.IsAny<JobExecutionContext>()), Times.Once);
        }
    }

    // Extension method to expose protected DoWorkAsync for testing
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
