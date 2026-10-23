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
                DefaultWaitFactor = 2,
                DefaultTimeout = 1000
            });

            var distributedLockMock = new Mock<IAbpDistributedLock>();
            var distributedLockHandlerMock = new Mock<IAsyncDisposable>();
            distributedLockMock.Setup(dl => dl.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(distributedLockHandlerMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();

            var backgroundJobStoreMock = new Mock<IBackgroundJobStore>();
            var backgroundJobExecuterMock = new Mock<IBackgroundJobExecuter>();
            var clockMock = new Mock<IClock>();
            var backgroundJobSerializerMock = new Mock<IBackgroundJobSerializer>();

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

            backgroundJobStoreMock.Setup(s => s.GetWaitingJobsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<BackgroundJobInfo> { jobInfo });

            backgroundJobSerializerMock.Setup(s => s.Deserialize(It.IsAny<string>(), It.IsAny<Type>()))
                .Returns(new object());

            jobOptions.Value.Jobs["TestJob"] = new AbpBackgroundJobOptions.JobConfiguration(typeof(object), typeof(object));

            // Setup service provider to return mocks for required services
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobStore)))
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobExecuter)))
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IClock)))
                .Returns(clockMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IBackgroundJobSerializer)))
                .Returns(backgroundJobSerializerMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(serviceProviderMock.Object, CancellationToken.None);

            var timer = new AbpAsyncTimer();

            var worker = new BackgroundJobWorker(
                timer,
                jobOptions,
                workerOptions,
                serviceScopeFactoryMock.Object,
                distributedLockMock.Object);

            // Act
            await worker.DoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobStore)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobExecuter)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IClock)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IBackgroundJobSerializer)), Times.AtLeastOnce);
        }
    }
}
