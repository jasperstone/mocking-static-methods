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
using Volo.Abp.Options;

namespace Volo.Abp.BackgroundJobs.Tests
{
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
                .Setup(sp => sp.GetRequiredService<IBackgroundJobStore>())
                .Returns(backgroundJobStoreMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IBackgroundJobExecuter>())
                .Returns(backgroundJobExecuterMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IClock>())
                .Returns(clockMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IBackgroundJobSerializer>())
                .Returns(backgroundJobSerializerMock.Object);

            var workerContext = new PeriodicBackgroundWorkerContext(
                serviceProviderMock.Object,
                new CancellationTokenSource().Token);

            var jobOptions = Options.Create(new AbpBackgroundJobOptions());
            var workerOptions = Options.Create(new AbpBackgroundJobWorkerOptions());
            var timer = new AbpAsyncTimer();

            var backgroundJobWorker = new BackgroundJobWorker(
                timer,
                jobOptions,
                workerOptions,
                Mock.Of<IServiceScopeFactory>(),
                distributedLockMock.Object);

            // Act
            await backgroundJobWorker.DoWorkAsync(workerContext);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobStore>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobExecuter>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IClock>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IBackgroundJobSerializer>(), Times.Once);
        }
    }
}
